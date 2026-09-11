import json
import csv

def extract_from_json(input_json: str, k: int,output_json: str) -> None:
    if k <= 0:
        print("k must be greater than 0!")
        return
    try:
        with open(input_json, "r", encoding="utf-8") as infile:
            with open(output_json, "w", encoding="utf-8") as outfile:
                for i, line in enumerate(infile):
                    if i >= k:
                        break

                    outfile.write(line)
        print(f"Extracting {k} rows of data from {input_json} to {output_json} successfully!")
    except Exception as ex:
        print(f"Error: {ex}")

def extract_from_csv(input_csv: str, k: int, output_csv: str) -> None:
    if k <= 0:
        print("k must be greater than 0!")
        return
    try:
        with open(input_csv, "r", encoding="utf-8") as infile:
            reader = csv.reader(infile)
            header = next(reader)
            with open(output_csv, "w", encoding="utf-8", newline="") as outfile:
                writer = csv.writer(outfile)
                writer.writerow(header)
                for i, row in enumerate(reader):
                    if i > k:
                        break

                    writer.writerow(row)

        print(f"Extracting {k} rows of data from {input_csv} to {output_csv} successfully!")
    except Exception as ex:
        print(f"Error: {ex}")

def main():
    input_reviews_books_json = "./data/reviews_Books_5.json"
    input_meta_books_json = "./data/meta_Books.json"
    input_ratings_books_csv = "./data/ratings_Books.csv"

    output_reviews_books_json = "./data/reviews_Books_5_sample.json"
    output_meta_books_json = "./data/meta_Books_sample.json"
    output_ratings_books_csv = "./data/ratings_Books_sample.csv"
    k = 1000

    extract_from_json(
        input_json=input_reviews_books_json,
        k = k,
        output_json=output_reviews_books_json
    )

    extract_from_json(
        input_json=input_meta_books_json,
        k = k,
        output_json=output_meta_books_json
    )
    extract_from_csv(
        input_csv=input_ratings_books_csv,
        k = k,
        output_csv=output_ratings_books_csv
    )


if __name__ == "__main__":
    main()

