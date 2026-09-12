# Data Visualization

## 1. Overview

## 2. Data Structure

### 2.1. Reviews Books

- This is user review books on Amazon.

| Name | Data Type | Description | Example | Note |
|---|---|---|---|---|
| `reviewerID` | `str` | Unique identifier of the user who wrote the review. | `"A10000012B7CGYKOMPQ4L"` | Used to identify reviewers. One user may write reviews for multiple books. |
| `asin` | `str` | Amazon Standard Identification Number (ASIN) identifying the book/product. | `"000100039X"` | Can be used to link reviews with the corresponding book in the Metadata dataset. |
| `reviewerName` | `str` | Display name of the reviewer. | `"Adam"` | This is the reviewer's name as provided in the dataset. |
| `helpful` | `object (list)` | Number of users who marked the review as helpful and the total number of users who voted on helpfulness. | `[0, 0]` | Typically represented as `[helpful_votes, total_votes]`. |
| `reviewText` | `str` | Full textual content of the review written by the reviewer. | `"Spiritually and mentally inspiring! A book that allows you to question your morals..."` | Main textual field for sentiment analysis, text mining, or NLP tasks. |
| `overall` | `int64` | Numerical rating given by the reviewer to the book. | `5` | Usually ranges from 1 to 5 stars. |
| `summary` | `str` | Short title or summary written by the reviewer for the review. | `"Wonderful!"` | Provides a concise description of the reviewer's opinion. |
| `unixReviewTime` | `int64` | Timestamp representing the time when the review was submitted, stored as Unix time. | `1355616000` | Can be converted to a standard date/time format. |
| `reviewTime` | `str` | Human-readable date when the review was submitted. | `"12 16, 2012"` | String representation of the review date. `unixReviewTime` is generally more suitable for date-based analysis. |
---

### 2.2. Metadata Books

| Name | Data Type | Description | Example | Note |
|---|---|---|---|---|
| `category` | `object (list)` | List of categories or classification labels associated with the book. | `[]` | May be empty when category information is unavailable. |
| `tech1` | `str` | Technical or product-related information associated with the book. | `""` | Often empty for books; availability depends on the original Amazon metadata. |
| `description` | `object (list)` | Textual description of the book/product. | `["It is a biology book with God's perspective."]` | Stored as a list and may contain one or multiple description entries. |
| `fit` | `str` | Product fit information. | `""` | More relevant to physical products such as clothing; generally empty or not meaningful for books. |
| `title` | `str` | Full title of the book. | `"Biology Gods Living Creation Third Edition 10 (A Beka Book Science Series)"` | Important field for identifying and describing the book. |
| `also_buy` | `object (list)` | List of ASINs of products that customers may also buy with this book. | `["0669009075", "B000K2P5SA", "B00MD4G2N0"]` | Contains references to other Amazon products/books. |
| `tech2` | `str` | Additional technical or product-related information. | `""` | Usually empty for books. |
| `brand` | `str` | Brand, publisher, author, or other entity associated with the product in the metadata. | `"Keith Graham"` | The meaning can vary depending on the original Amazon metadata. |
| `feature` | `object (list)` | List of product features or highlighted characteristics. | `[]` | Often empty for books. |
| `rank` | `str` | Amazon sales rank information for the book/product. | `"1,349,781 in Books ("` | Stored as text and may contain additional category information. |
| `also_view` | `object (list)` | List of ASINs of products that customers also viewed. | `["0019777701", "B000AUCX7I", "B000K2P5SA"]` | Contains references to other products/books. |
| `main_cat` | `str` | Main product category assigned to the item. | `"Books"` | Indicates the main category of the product. |
| `similar_item` | `str` | Information about similar items recommended by Amazon. | `""` | May be empty when no information is available. |
| `date` | `datetime64[s]` | Date associated with the product metadata. | `NaT` | In the provided dataset, all 1,000 records have missing values for this field. |
| `price` | `str` | Listed price of the book/product. | `"$39.94"` | Stored as a string because it contains the currency symbol. |
| `asin` | `str` | Amazon Standard Identification Number (ASIN) identifying the book/product. | `"0000092878"` | Primary field for linking metadata with reviews and ratings. |
| `imageURL` | `object (list)` | List of URLs pointing to product/book images. | `[]` | May be empty when images are unavailable. |
| `imageURLHighRes` | `object (list)` | List of URLs pointing to high-resolution product/book images. | `[]` | May be empty when high-resolution images are unavailable. |
---

### 2.3. Ratings Books

| Name | Data Type | Description | Example | Note |
|---|---|---|---|---|
| `reviewerID` | `str` | Unique identifier of the user who gave the rating. | `"AH2L9G3DQHHAJ"` | Can be used to identify the user associated with a rating. |
| `asin` | `str` | Amazon Standard Identification Number (ASIN) identifying the rated book/product. | `"0000000116"` | Can be used to link the rating with the corresponding book in Metadata. |
| `overall` | `float64` | Numerical rating assigned by the user to the book. | `4.0` | Represents the user's rating, typically on a 1–5 scale. |
| `unixReviewTime` | `int64` | Unix timestamp indicating when the rating/review was created. | `1019865600` | Can be converted into a human-readable date/time. |

---

### 2.4. Relationship Between the Three DataFrames

The three datasets can be understood as follows:

```text

                    ┌─────────────────────┐
                    │   Metadata Books    │
                    │                     │
                    │ asin                │
                    │ title               │
                    │ brand               │
                    │ category            │
                    │ price               │
                    │ description         │
                    │ ...                 │
                    └──────────┬──────────┘
                               │
                              asin
                               │
                 ┌─────────────┴─────────────┐
                 │                           │
                 ▼                           ▼
       ┌──────────────────┐        ┌──────────────────┐
       │   Reviews Books  │        │   Ratings Books  │
       │                  │        │                  │
       │ reviewerID       │        │ reviewerID       │
       │ asin             │        │ asin             │
       │ reviewText       │        │ overall          │
       │ summary          │        │ unixReviewTime   │
       │ overall          │        │                  │
       │ reviewTime       │        └──────────────────┘
       │ helpful          │
       │ ...              │
       └──────────────────┘
```