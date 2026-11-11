using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace back_end.Services.Implements
{
    internal sealed record ParsedBookImportRow(
        int RowNumber,
        Dictionary<string, string?> Values,
        string? ParseError = null);

    internal static class BookImportFileParser
    {
        private const long MaxFileSize = 10 * 1024 * 1024;

        public static async Task<List<ParsedBookImportRow>> ParseAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidDataException("The uploaded file is empty.");
            }
            if (file.Length > MaxFileSize)
            {
                throw new InvalidDataException("The uploaded file exceeds the 10 MB limit.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            await using Stream stream = file.OpenReadStream();
            return extension switch
            {
                ".csv" => ParseCsv(await ReadTextAsync(stream)),
                ".json" => ParseJson(await ReadTextAsync(stream)),
                ".xlsx" => ParseXlsx(stream),
                ".xls" => throw new InvalidDataException("Legacy .xls files are not supported. Save the workbook as .xlsx and upload it again."),
                _ => throw new InvalidDataException("Supported file formats are CSV, .xlsx, and JSON.")
            };
        }

        private static async Task<string> ReadTextAsync(Stream stream)
        {
            using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync();
        }

        private static List<ParsedBookImportRow> ParseJson(string text)
        {
            using JsonDocument document = JsonDocument.Parse(text);
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("books", out JsonElement books))
            {
                root = books;
            }

            IEnumerable<JsonElement> elements = root.ValueKind switch
            {
                JsonValueKind.Array => root.EnumerateArray(),
                JsonValueKind.Object => new[] { root },
                _ => throw new InvalidDataException("JSON must contain a book object or an array of book objects.")
            };

            List<ParsedBookImportRow> rows = new();
            int rowNumber = 1;
            foreach (JsonElement element in elements)
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    rows.Add(new ParsedBookImportRow(rowNumber++, new(), "Each JSON row must be an object."));
                    continue;
                }

                Dictionary<string, string?> values = new(StringComparer.OrdinalIgnoreCase);
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string? value = property.Value.ValueKind switch
                    {
                        JsonValueKind.Null or JsonValueKind.Undefined => null,
                        JsonValueKind.Array => string.Join("|", property.Value.EnumerateArray().Select(item => item.ToString())),
                        JsonValueKind.Object => property.Value.GetRawText(),
                        _ => property.Value.ToString()
                    };
                    values[NormalizeHeader(property.Name)] = value;
                }
                rows.Add(new ParsedBookImportRow(rowNumber++, values));
            }
            return rows;
        }

        private static List<ParsedBookImportRow> ParseCsv(string text)
        {
            List<(List<string> Fields, int StartLine)> records = new();
            List<string> fields = new();
            StringBuilder field = new();
            bool inQuotes = false;
            int line = 1;
            int recordStartLine = 1;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else if ((ch == '\r' || ch == '\n') && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    records.Add((fields, recordStartLine));
                    fields = new List<string>();
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    line++;
                    recordStartLine = line;
                }
                else
                {
                    field.Append(ch);
                    if (ch == '\n') line++;
                }
            }

            if (inQuotes)
            {
                throw new InvalidDataException("CSV contains an unterminated quoted field.");
            }
            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                records.Add((fields, recordStartLine));
            }
            if (records.Count < 2)
            {
                throw new InvalidDataException("CSV must include a header row and at least one book row.");
            }

            string[] headers = records[0].Fields.Select(NormalizeHeader).ToArray();
            return records.Skip(1)
                .Where(record => record.Fields.Any(value => !string.IsNullOrWhiteSpace(value)))
                .Select(record => new ParsedBookImportRow(
                    record.StartLine,
                    MapFields(headers, record.Fields),
                    record.Fields.Count != headers.Length
                        ? $"Expected {headers.Length} columns but found {record.Fields.Count}."
                        : null))
                .ToList();
        }

        private static List<ParsedBookImportRow> ParseXlsx(Stream stream)
        {
            using ZipArchive archive = new(stream, ZipArchiveMode.Read, leaveOpen: true);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            ZipArchiveEntry? sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (sheetEntry == null)
            {
                throw new InvalidDataException("The workbook does not contain a readable first worksheet.");
            }

            List<string> sharedStrings = new();
            ZipArchiveEntry? stringsEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (stringsEntry != null)
            {
                using Stream stringsStream = stringsEntry.Open();
                XDocument stringsDocument = XDocument.Load(stringsStream);
                sharedStrings = stringsDocument.Descendants(ns + "si")
                    .Select(item => string.Concat(item.Descendants(ns + "t").Select(textNode => textNode.Value)))
                    .ToList();
            }

            using Stream sheetStream = sheetEntry.Open();
            XDocument sheet = XDocument.Load(sheetStream);
            List<(int RowNumber, Dictionary<int, string> Cells)> rows = new();
            foreach (XElement row in sheet.Descendants(ns + "row"))
            {
                int rowNumber = (int?)row.Attribute("r") ?? rows.Count + 1;
                Dictionary<int, string> cells = new();
                foreach (XElement cell in row.Elements(ns + "c"))
                {
                    string reference = (string?)cell.Attribute("r") ?? string.Empty;
                    int column = GetColumnIndex(reference);
                    if (column < 0) continue;
                    string type = (string?)cell.Attribute("t") ?? string.Empty;
                    string value;
                    if (type == "inlineStr")
                    {
                        value = string.Concat(cell.Descendants(ns + "t").Select(textNode => textNode.Value));
                    }
                    else
                    {
                        value = cell.Element(ns + "v")?.Value ?? string.Empty;
                        if (type == "s" && int.TryParse(value, out int stringIndex) &&
                            stringIndex >= 0 && stringIndex < sharedStrings.Count)
                        {
                            value = sharedStrings[stringIndex];
                        }
                    }
                    cells[column] = value;
                }
                rows.Add((rowNumber, cells));
            }

            if (rows.Count < 2)
            {
                throw new InvalidDataException("The first worksheet must include a header row and at least one book row.");
            }

            int maxColumn = rows.Max(row => row.Cells.Keys.DefaultIfEmpty(-1).Max());
            string[] headers = Enumerable.Range(0, maxColumn + 1)
                .Select(column => NormalizeHeader(rows[0].Cells.GetValueOrDefault(column, string.Empty)))
                .ToArray();

            return rows.Skip(1)
                .Where(row => row.Cells.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
                .Select(row => new ParsedBookImportRow(
                    row.RowNumber,
                    MapFields(headers, Enumerable.Range(0, headers.Length)
                        .Select(column => row.Cells.GetValueOrDefault(column, string.Empty)).ToList())))
                .ToList();
        }

        private static Dictionary<string, string?> MapFields(
            IReadOnlyList<string> headers,
            IReadOnlyList<string> values)
        {
            Dictionary<string, string?> result = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Math.Min(headers.Count, values.Count); i++)
            {
                if (!string.IsNullOrEmpty(headers[i])) result[headers[i]] = values[i].Trim();
            }
            return result;
        }

        internal static string NormalizeHeader(string value) =>
            new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

        private static int GetColumnIndex(string cellReference)
        {
            int value = 0;
            bool foundLetter = false;
            foreach (char ch in cellReference)
            {
                if (!char.IsLetter(ch)) break;
                foundLetter = true;
                value = checked(value * 26 + char.ToUpperInvariant(ch) - 'A' + 1);
            }
            return foundLetter ? value - 1 : -1;
        }
    }
}
