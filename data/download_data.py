import requests
from pathlib import Path

DATA_DIR = Path("data")

DATASETS = {
    "reviews_books.json.gz":
        "http://snap.stanford.edu/data/amazon/productGraph/categoryFiles/reviews_Books_5.json.gz",

    "meta_books.json.gz":
        "https://mcauleylab.ucsd.edu/public_datasets/data/amazon_v2/metaFiles2/meta_Books.json.gz",

    "ratings_books.csv":
        "http://snap.stanford.edu/data/amazon/productGraph/categoryFiles/ratings_Books.csv",
}

def download_file(url: str, output_path: Path) -> None:
    print("=" * 70)
    print(f"Downloading: {output_path.name}")
    print(f"URL       : {url}")
    print("=" * 70)

    try:
        with requests.get(url, stream=True, timeout=60) as response:
            response.raise_for_status()

            total_size = int(response.headers.get("content-length", 0))
            downloaded = 0

            with open(output_path, "wb") as file:
                for chunk in response.iter_content(chunk_size=1024 * 1024):
                    if not chunk:
                        continue

                    file.write(chunk)
                    downloaded += len(chunk)

                    if total_size > 0:
                        percentage = downloaded / total_size * 100
                        downloaded_mb = downloaded / (1024 ** 2)
                        total_mb = total_size / (1024 ** 2)

                        print(
                            f"\rProgress: {percentage:6.2f}% "
                            f"({downloaded_mb:.2f} / {total_mb:.2f} MB)",
                            end=""
                        )
                    else:
                        downloaded_mb = downloaded / (1024 ** 2)
                        print(
                            f"\rDownloaded: {downloaded_mb:.2f} MB",
                            end=""
                        )

        print("\nDownload completed.")
        print(f"Saved to: {output_path}")

    except requests.RequestException as e:
        print(f"\nDownload failed: {e}")

        # Remove incomplete file
        if output_path.exists():
            output_path.unlink()

        raise

def main() -> None:
    DATA_DIR.mkdir(parents=True, exist_ok=True)

    print("Amazon Books Dataset Downloader")
    print()

    for filename, url in DATASETS.items():
        output_path = DATA_DIR / filename

        # Skip if file already exists
        if output_path.exists():
            print(f"[SKIP] {filename} already exists.")
            continue

        download_file(url, output_path)

    print()
    print("=" * 70)
    print("All datasets downloaded successfully.")
    print("=" * 70)

    print("\nFiles:")
    for file in DATA_DIR.iterdir():
        size_mb = file.stat().st_size / (1024 ** 2)
        print(f"  - {file.name:<30} {size_mb:>10.2f} MB")


if __name__ == "__main__":
    main()
