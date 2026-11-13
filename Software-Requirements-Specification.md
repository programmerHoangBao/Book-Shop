# SOFTWARE REQUIREMENTS SPECIFICATION (SRS)

## BOOK SHOP — ONLINE BOOKSTORE PLATFORM

**Version:** 1.0  
**Status:** Draft for Review  
**Document Type:** Software Requirements Specification  
**Source Document:** Business Requirements Document (BRD) v1.0  
**Owner:** Founder & CEO — Book Shop

---

# TABLE OF CONTENTS

1. [Introduction](#1-introduction)
2. [System Overview](#2-system-overview)
3. [User Types and Roles](#3-user-types-and-roles)
4. [System Scope](#4-system-scope)
5. [Functional Requirements](#5-functional-requirements)
   - 5.1. Account Management
   - 5.2. Book and Category Management
   - 5.3. Book Search and Filtering
   - 5.4. Book Details
   - 5.5. Shopping Cart
   - 5.6. Order Placement
   - 5.7. Payment
   - 5.8. Order Management
   - 5.9. Returns and Refunds
   - 5.10. Inventory Management
   - 5.11. Store Management
   - 5.12. Promotions
   - 5.13. Product Reviews
   - 5.14. Notifications
   - 5.15. Chatbot
   - 5.16. Business Reporting
6. [Business Requirements and Business Rules](#6-business-requirements-and-business-rules)
7. [Non-functional Requirements](#7-non-functional-requirements)
8. [Data Requirements](#8-data-requirements)
9. [Business Processes and States](#9-business-processes-and-states)
10. [Authorization and Access Control](#10-authorization-and-access-control)
11. [User Interface and User Experience Requirements](#11-user-interface-and-user-experience-requirements)
12. [Integration Requirements](#12-integration-requirements)
13. [Reporting Requirements](#13-reporting-requirements)
14. [Acceptance Criteria](#14-acceptance-criteria)
15. [Traceability Matrix](#15-traceability-matrix)
16. [Open Issues and Clarifications](#16-open-issues-and-clarifications)
17. [Out of Scope](#17-out-of-scope)
18. [Appendices](#18-appendices)

---

# 1. INTRODUCTION

## 1.1. Document Purpose

This Software Requirements Specification (SRS) describes the software requirements for **Book Shop — an Online Bookstore Platform**.

The SRS translates the business needs identified in the BRD into requirements that the software system must satisfy.

This document serves as the basis for:

- System analysis and design.
- Software architecture design.
- Database design.
- Feature development.
- API design.
- User interface design.
- Test case preparation and system testing.
- Product acceptance.

## 1.2. Reference Document

Primary source document:

**Business Requirements Document — Book Shop, Version 1.0**

The BRD defines the business needs, user types, and overall scope of the system.

## 1.3. Document Scope

This SRS covers requirements related to:

- User accounts.
- Books and categories.
- Search.
- Shopping carts.
- Order placement.
- Payment.
- Orders.
- Returns and refunds.
- Inventory.
- Stores.
- Promotions.
- Reviews.
- Notifications.
- Chatbot.
- Reporting.
- Authorization.
- System-level non-functional requirements.

---

# 2. SYSTEM OVERVIEW

## 2.1. System Objectives

Book Shop is an online bookstore platform intended to expand business operations from physical stores to the online environment.

The system provides an online sales channel while supporting the management of:

- Customers.
- Books.
- Orders.
- Payments.
- Shipping.
- Inventory.
- Stores.
- Promotions.
- Business operations.

The overall objective is to connect **customers — website — stores — warehouses — business operations** within a single platform.

## 2.2. Business Objectives

The system must support Book Shop in achieving the following objectives:

1. Increase revenue from online sales.
2. Expand customer reach.
3. Improve the book-shopping experience.
4. Manage products and inventory accurately.
5. Reduce order-processing time.
6. Provide reports to support management decisions.
7. Establish a platform that can be expanded in the future.

## 2.3. Actors Overview

The system has the following user groups:

| Actor         | Description                                                                                    |
| ------------- | ---------------------------------------------------------------------------------------------- |
| Guest         | A user who is not signed in and can browse and search for books.                               |
| Customer      | A user who has an account.                                                                     |
| Employee      | An employee who processes orders and confirms delivery statuses.                               |
| Store Manager | A manager responsible for merchandise, orders, and business information for an assigned store. |
| Administrator | A user who manages all activities permitted by Book Shop.                                      |

These actors are defined in the BRD.

---

# 3. USER TYPES AND ROLES

## 3.1. Guest

A Guest can:

- View the book catalog.
- Search for books.
- Filter books.
- Sort books.
- View book details.
- Explore products.

A Guest cannot use features that require an account.

## 3.2. Customer

A Customer can:

- Register an account.
- Sign in.
- Verify their account.
- Reset a forgotten password.
- Update personal information.
- Manage addresses.
- Manage the shopping cart.
- Place orders.
- Track orders.
- Cancel orders when eligible.
- Submit return/refund requests.
- Review products.
- Receive notifications.
- Use the chatbot.

## 3.3. Employee

An Employee can:

- Search for orders.
- Confirm orders.
- Process orders.
- Hand orders over to a third-party shipping provider.
- Receive delivery-status information from the third party.
- Confirm successful delivery to the customer.
- Cancel orders in accordance with policy.
- Process return and refund requests according to assigned permissions.

## 3.4. Store Manager

A Store Manager can:

- View inventory for the assigned store.
- Perform stocktaking.
- Receive stock.
- Issue stock.
- Transfer stock.
- Receive stock from another store.
- Track change history.
- Manage orders within the scope of their permissions.
- View business information within the scope of the assigned store.

## 3.5. Administrator

An Administrator has permission to manage all activities permitted by Book Shop, including:

- Users.
- Products.
- Categories.
- Orders.
- Inventory.
- Stores.
- Promotions.
- Reviews.
- Reports.
- Other administrative configurations.

---

# 4. SYSTEM SCOPE

## 4.1. In Scope

The system includes:

- Account Management.
- Product and Category Management.
- Search and Filtering.
- Shopping Cart.
- Order Management.
- Payment.
- Inventory Management.
- Store Management.
- Promotion Management.
- Product Reviews.
- Notifications.
- Customer Chatbot.
- Business Reporting.

These features are within the scope defined in the BRD.

## 4.2. Out of Scope

The following features are not included in the initial version:

- A marketplace for third-party sellers.
- Livestream shopping.
- Book rental.
- A comprehensive accounting/ERP system.
- A dedicated mobile application.
- Management of the order-shipping operation.
- Online bank payments or other online payment methods.

Shipping is handled by a third party. Version 1.0 supports Cash on Delivery only.

---

# 5. FUNCTIONAL REQUIREMENTS

# 5.1. Account Management

## FR-ACC-001 — Account Registration

**Actor:** Guest

The system must allow Guests to register an account.

### Preconditions

- The user does not already have an account or is accessing the registration feature.

### Main Flow

1. The Guest opens the registration feature.
2. The Guest enters the required information.
3. The system validates the submitted data.
4. The system creates the account.
5. The system requests account verification if required.
6. The system displays the result.

In addition to registering by entering information, a Guest can also register using Google (Google Sign-In).

### Business Rules

- Account information must be valid.
- The account must be verified according to Book Shop's process.

---

## FR-ACC-002 — Sign In

**Actor:** Customer

The system must allow Customers to sign in to their accounts.

### Main Flow

1. The Customer enters their sign-in credentials.
2. The system authenticates the credentials.
3. If the credentials are valid, the system grants access to the account.
4. If the credentials are invalid, the system displays an error.

Customers can also sign in with Google without entering their credentials manually.

---

## FR-ACC-003 — Account Verification

The system must support account verification after registration. The system may verify a user's account using a One-Time Password (OTP).

---

## FR-ACC-004 — Password Recovery

Customers must be able to recover a forgotten password by verifying their email address with an OTP and providing a new password.

---

## FR-ACC-005 — Update Personal Information

Customers must be able to:

- View their personal information.
- Update permitted information.
- Save the updated information.

---

## FR-ACC-006 — Manage Shipping Addresses

Customers must be able to:

- Add an address.
- View addresses.
- Edit an address.
- Select an address to use when placing an order.

---

## FR-ACC-007 — View Purchase History

Customers must be able to view the history of orders they have placed.

---

# 5.2. Book and Category Management

## FR-PROD-001 — Add a Book

**Actor:** Administrator

An Administrator can add a new product.

Minimum information:

- Book title.
- Author.
- Publisher.
- Category.
- Description.
- Images.
- Selling price.

These information fields are defined in the BRD.

---

## FR-PROD-002 — View Books

The system must allow users to view book information in accordance with their access permissions.

---

## FR-PROD-003 — Edit a Book

An Administrator can edit product information.

---

## FR-PROD-004 — Discontinue a Book

An Administrator can discontinue a product.

A discontinued product must not be displayed as available for purchase.

---

## FR-PROD-005 — Reactivate a Book

An Administrator can reactivate a discontinued product.

---

# 5.3. Book Search and Filtering

## FR-SEARCH-001 — Search

The system must support searching by:

- Book title.
- Author.
- Publisher.
- Category.
- Related keywords.

## FR-SEARCH-002 — Filter

The system must support filtering by:

- Price.
- Category.
- Publisher.
- Rating.
- Stock availability.

## FR-SEARCH-003 — Sort

The system must support sorting by:

- Price.
- Best-selling.
- Rating.
- Newest books.

The search, filtering, and sorting criteria above are taken from the BRD.

---

# 5.4. Book Details

## FR-PROD-006 — Display Book Details

The system must display:

- Book title.
- Author.
- Images.
- Price.
- Promotional price, if applicable.
- Description.
- Publisher.
- Rating.
- Stock availability.

## FR-PROD-007 — Add to Cart

Customers can add a book to their shopping cart from the book details page.

## FR-PROD-008 — Buy Now

Customers can purchase a product directly from the book details page.

---

# 5.5. Shopping Cart

## FR-CART-001 — Add Products

Customers can add books to their shopping carts.

## FR-CART-002 — Change Quantities

Customers can increase or decrease the quantity of books in their carts.

## FR-CART-003 — Remove Products

Customers can remove books from their shopping carts.

## FR-CART-004 — View Total Amount

The system must display the total value of the shopping cart.

## FR-CART-005 — Validate Inventory

The system must not allow Customers to order a quantity greater than the quantity available for sale.

This is a mandatory business requirement defined in the BRD.

---

# 5.6. Order Placement

## FR-ORDER-001 — Create an Order

Customers can create an order through the following process:

```text
Search for books
      ↓
Select books
      ↓
Add to cart (optional)
      ↓
Enter shipping address
      ↓
Select payment method
      ↓
Apply promotion (optional)
      ↓
Confirm order
      ↓
Pending Confirmation
      ↓
Shipping
      ↓
Completed
```

The process above is defined in the BRD.

## FR-ORDER-002 — Calculate the Total Amount

Before a Customer confirms an order, the system must clearly display the total amount payable.

The total amount must reflect:

- Product prices.
- Quantities.
- Promotional prices, if applicable.
- Other applicable charges supported by Book Shop.

## FR-ORDER-003 — Validate Inventory Before Ordering

The system must check the quantity available for sale before creating an order.

## FR-ORDER-004 — Apply Promotions

Customers can apply valid promotions or discount codes.

## FR-ORDER-005 — Confirm an Order

After the Customer confirms the order, the order is created with the following status:

**Pending Confirmation**

---

# 5.7. Payment

## FR-PAY-001 — Cash on Delivery

Version 1.0 must support the following payment method:

**Cash on Delivery (COD).**

The Customer pays the third-party shipping provider upon receiving the order.

## FR-PAY-002 — Confirm Payment Completion

After the third party delivers the order and Book Shop receives the payment, the system must allow the order to be confirmed as completed.

## FR-PAY-003 — Online Payments Not Supported

Version 1.0 does not include:

- Online bank payments.
- Other online payment methods.

---

# 5.8. Order Management

## FR-ORDER-006 — Customer Views Orders

Customers can:

- View their order list.
- View order details.
- View ordered products.
- View amounts.
- Track order statuses.

## FR-ORDER-007 — Cancel an Order

Customers can cancel an order if its status and the applicable policy allow cancellation.

**An order cannot be cancelled when its status is "Shipping".**

## FR-ORDER-008 — Employee Searches for Orders

Employees can search for orders.

## FR-ORDER-009 — Employee Confirms an Order

Employees can confirm orders.

An Employee confirms that an order has moved to the **Shipping** status.

An Employee confirms that an order is **Completed** after receiving the payment from the third-party shipping provider.

## FR-ORDER-010 — Process an Order

Employees can process orders according to the business process.

## FR-ORDER-011 — Employee Cancels an Order

Employees can cancel orders in accordance with policy.

---

# 5.9. Returns and Refunds

## FR-RETURN-001 — Create a Return Request

Customers can request a return after receiving the order, provided that the order is in the **Completed** status. The return request must be submitted no later than 3 days after the order is received.

Customers must provide:

- Supporting evidence.
- Bank account information.

## FR-RETURN-002 — Review a Request

Book Shop can review return requests.

## FR-RETURN-003 — Approve or Reject a Request

Book Shop must be able to:

- Approve a request.
- Reject a request.

## FR-RETURN-004 — Receive and Inspect Returned Products

Book Shop receives and inspects returned products.

## FR-RETURN-005 — Issue a Refund

If a request meets the eligibility requirements, the system must support recording the refund.

## FR-RETURN-006 — Update the Order and Products

After processing a return, the system must update:

- Order status.
- Product condition.
- Refund information.

The process is defined in the BRD.

---

# 5.10. Inventory Management

## FR-INV-001 — Track Inventory

The system must track book quantities at each store or warehouse.

Each product must have the following information:

- Current quantity.
- Quantity reserved for orders.
- Quantity available for sale.
- Damaged quantity.
- Quantity pending receipt.

## FR-INV-002 — Update Inventory

Inventory must be updated when any of the following events occur:

- Stock receipt.
- Sale.
- Order placement.
- Order cancellation.
- Returned goods.
- Stock transfer.
- Recording of damaged goods.

## FR-INV-003 — Prevent Overselling

The system must never allow sales that exceed the quantity available for sale.

---

# 5.11. Store Management

## FR-STORE-001 — View Store Inventory

Authorized users can view inventory for each store.

## FR-STORE-002 — Stocktaking

The system must support recording stocktaking activities.

## FR-STORE-003 — Receive Stock

The system must support recording received stock.

## FR-STORE-004 — Issue Stock

The system must support recording issued stock.

## FR-STORE-005 — Transfer Stock

The system must support transferring stock between stores/warehouses.

## FR-STORE-006 — Receive Transferred Stock

The receiving store can record the receipt of transferred stock.

## FR-STORE-007 — Inventory History

The system must store and allow users to track inventory change history.

---

# 5.12. Promotions

## FR-PROMO-001 — Percentage-Based Promotions

The system supports percentage discounts.

## FR-PROMO-002 — Fixed-Amount Promotions

The system supports discounts of a fixed monetary amount.

## FR-PROMO-003 — Discount Codes

The system supports discount codes.

## FR-PROMO-004 — Free Shipping

The system supports free-shipping promotions.

## FR-PROMO-005 — Flash Sales

The system supports Flash Sale campaigns.

## FR-PROMO-006 — Category-Based Promotions

The system supports promotions for specific book categories.

## FR-PROMO-007 — Validate Promotion Conditions

Promotions must be validated against:

- Products.
- Order value.
- Validity period.
- Number of uses.

Expired promotions must not be applied.

---

# 5.13. Product Reviews

## FR-REVIEW-001 — Star Ratings

Customers can rate products from:

**1 to 5 stars.**

## FR-REVIEW-002 — Written Reviews

Customers can write comments about products.

## FR-REVIEW-003 — Review Eligibility Check

Book Shop may require that only Customers who have purchased a product can review it.

## FR-REVIEW-004 — Moderation

Administrators, Store Managers, and Employees can:

- View reviews.
- Moderate reviews.
- Hide reviews that violate policy.

---

# 5.14. Notifications

## FR-NOTI-001 — Event Notifications

The system must support notifying Customers about important events, including:

- Account registration.
- Order placement.
- Payment.
- Order confirmation.
- Shipping.
- Order cancellation.
- Refund.
- Promotions.

**Notification channels:** In-app notifications and email.

---

# 5.15. Chatbot

## FR-CHAT-001 — Search for Books

The chatbot must help Customers search for books.

## FR-CHAT-002 — Recommend Books

The chatbot must be able to recommend books.

## FR-CHAT-003 — Answer Product-Related Questions

The chatbot must support answering questions related to products.

## FR-CHAT-004 — Future Expansion

The system may be expanded into a personalized book recommendation system.

Personalized recommendations are not a mandatory requirement for the initial version.

---

# 5.16. Business Reporting

## FR-REPORT-001 — Revenue Reports

The system must provide revenue information by:

- Day.
- Week.
- Month.
- Quarter.
- Year.
- Store.
- Product.
- Category.

The system may export order reports to CSV files and email reports daily at **08:00 AM**. Reports must be sent only to the Administrator.

## FR-REPORT-002 — Order Reports

The system must provide information about:

- New orders.
- Orders being processed.
- Completed orders.
- Cancelled orders.

## FR-REPORT-003 — Product and Inventory Reports

The system must provide information about:

- Books with low stock.
- Out-of-stock books.
- Best-selling books.
- Slow-moving books.

## FR-REPORT-004 — Customer Reports

The system must provide information about:

- New customers.
- Returning customers.
- Purchase frequency.

---

# 6. BUSINESS REQUIREMENTS AND BUSINESS RULES

| ID     | Business Rule                                                          | System Requirement                                                        |
| ------ | ---------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| BR-001 | The selling price must not be less than 0.                             | The system must reject a selling price below 0.                           |
| BR-002 | Orders must not exceed the quantity available for sale.                | The system must validate the available quantity.                          |
| BR-003 | Expired promotions cannot be applied.                                  | The system must validate the promotion's validity period.                 |
| BR-004 | Users must not redeem more points than they currently have.            | If the system supports points, it must validate the user's point balance. |
| BR-005 | Reviews must comply with the applicable policy.                        | The system must enforce the review policy.                                |
| BR-006 | An order can only be cancelled when the applicable conditions are met. | The system must validate the order status and cancellation policy.        |
| BR-007 | Refunds must comply with the return policy.                            | The system must validate refund eligibility.                              |
| BR-008 | Users may only perform tasks appropriate to their responsibilities.    | The system must enforce authorization.                                    |
| BR-009 | The total amount must be clearly determined before an order is placed. | Checkout must display the total amount.                                   |
| BR-010 | Orders cannot be cancelled while they are being shipped.               | The system must reject cancellation attempts during shipping.             |

The business rules above are derived from the BRD.

> **Note:** BR-004 regarding reward points appears in the BRD, but the functional scope does not describe a Loyalty/Points system. This requirement must therefore be clarified before implementation.

---

# 7. NON-FUNCTIONAL REQUIREMENTS

The BRD does not specify concrete technical metrics for performance, load capacity, SLA, or technical security. Therefore, the requirements below are identified as **system requirements that need further clarification**, rather than assigning unsupported technical values.

## NFR-001 — Performance

The system must provide response times appropriate for the online shopping experience.

- The response time of a typical API request must not exceed **500 ms**.
- The system must be able to handle at least **100 requests per second (RPS)** under normal load conditions.
- The system must support at least **500 concurrent users** without becoming unavailable or experiencing severe performance degradation.

## NFR-002 — Availability

The system must provide an appropriate level of availability to support business operations.

## NFR-003 — Scalability

The system must be scalable as the number of the following increases:

- Customers.
- Products.
- Orders.
- Stores.

## NFR-004 — Security

The system must protect:

- Account information.
- Personal information.
- Shipping addresses.
- Order-related information.
- Bank account information used for refunds.

Specific technical security mechanisms: **TBD**.

## NFR-005 — Authorization

Users may only access features and data that are appropriate to their assigned responsibilities.

## NFR-006 — Data Integrity

The system must ensure the consistency of:

- Orders.
- Inventory.
- Payments.
- Promotions.
- Returns.
- Refunds.

## NFR-007 — Auditability

Important changes to inventory and administrative activities must be traceable.

Audit log details: **TBD**.

## NFR-008 — Usability

The user interface must enable users to complete the book-purchasing process clearly and conveniently.

## NFR-009 — Maintainability

The system must be maintainable and extensible in subsequent phases.

## NFR-010 — Compatibility

The web platform must work on browsers supported by Book Shop.

---

# 8. DATA REQUIREMENTS

## 8.1. Product

| Field         | Description            | Required |
| ------------- | ---------------------- | -------- |
| Book ID       | Unique book identifier | Yes      |
| Book Name     | Book title             | Yes      |
| Author        | Author                 | No       |
| Publisher     | Publisher              | Yes      |
| Category      | Book category          | Yes      |
| Description   | Book description       | No       |
| Image         | Book image             | No       |
| Selling Price | Selling price          | Yes      |
| Status        | Sales status           | Yes      |

## 8.2. Customer

Customer information must support:

- Account information.
- Personal information.
- Shipping addresses.
- Purchase history.

## 8.3. Order

At a minimum, an order must manage:

- Order ID.
- Customer.
- Products.
- Quantities.
- Prices.
- Promotions.
- Total amount.
- Shipping address.
- Payment method.
- Order status.
- Shipping-related information.
- Refund information, if applicable.

## 8.4. Inventory

Inventory must manage:

- Current quantity.
- Reserved quantity.
- Available quantity.
- Damaged quantity.
- Quantity pending receipt.
- Store/warehouse.
- Change history.

## 8.5. Review

A review must contain at least:

- Reviewer.
- Product.
- Star rating.
- Review content.
- Moderation status.

## 8.6. Promotion

A promotion must manage:

- Promotion type.
- Discount value.
- Applicable products/categories.
- Order conditions.
- Validity period.
- Number of uses.

---

# 9. BUSINESS PROCESSES AND STATES

# 9.1. Order Lifecycle

Minimum order statuses:

```text
Pending Confirmation
        ↓
Shipping
        ↓
Completed
```

An order may also have the following status:

```text
Cancelled
```

subject to the applicable business conditions.

## 9.2. Order Cancellation Rule

A Customer is allowed to cancel an order when:

```text
Order Status != Shipping
AND
Cancellation Policy = Allowed
```

A Customer is not allowed to cancel an order when:

```text
Order Status = Shipping
```

## 9.3. Return and Refund Lifecycle

```text
Customer submits a request
        ↓
Book Shop reviews the request
        ↓
    ┌───┴───┐
    ↓       ↓
 Approved  Rejected
    ↓
Receive returned product
    ↓
Inspect product
    ↓
Issue refund
    ↓
Update order and product information
```

---

# 10. AUTHORIZATION AND ACCESS CONTROL

The SRS-level permission matrix is as follows:

| Feature              | Guest | Customer | Employee | Store Manager | Admin |
| -------------------- | ----: | -------: | -------: | ------------: | ----: |
| View books           |     ✓ |        ✓ |        ✓ |             ✓ |     ✓ |
| Search books         |     ✓ |        ✓ |        ✓ |             ✓ |     ✓ |
| Manage shopping cart |     - |        ✓ |        - |             - |     - |
| Place orders         |     - |        ✓ |        - |             - |     - |
| View own orders      |     - |        ✓ |        - |             - |     - |
| Process orders       |     - |        - |        ✓ |           ✓\* |     ✓ |
| Manage products      |     - |        - |        - |             - |     ✓ |
| Manage inventory     |     - |        - |        - |             ✓ |     ✓ |
| Manage stores        |     - |        - |        - |           ✓\* |     ✓ |
| Manage promotions    |     - |        - |        - |             - |     ✓ |
| Moderate reviews     |     - |        - |        - |             - |     ✓ |
| Reports              |     - |        - |      TBD |           ✓\* |     ✓ |
| Chatbot              |     - |        ✓ |        - |             - |     ✓ |

`*` Permissions must be restricted according to the assigned store and responsibilities.

General principle:

> Users may only perform tasks that are appropriate to the responsibilities assigned to them.

This corresponds to BR-008 in the BRD.

---

# 11. USER INTERFACE AND USER EXPERIENCE REQUIREMENTS

## 11.1. Customer-facing Pages

The system should provide the following groups of screens:

### Public

- Home.
- Book Listing.
- Search Results.
- Book Details.
- Sign In.
- Registration.

### Customer

- Profile.
- Address Management.
- Shopping Cart.
- Checkout.
- Order History.
- Order Details.
- Return/Refund Request.
- Reviews.
- Notifications.
- Chatbot.

### Back Office

- Dashboard.
- Product Management.
- Category Management.
- Order Management.
- Inventory Management.
- Store Management.
- Promotion Management.
- Review Moderation.
- Business Reports.

## 11.2. Checkout

The checkout page must clearly display:

- Products.
- Quantities.
- Address.
- Payment method.
- Promotions.
- Total amount.
- Order confirmation button.

The total amount must be clearly determined before the Customer places the order.

---

# 12. INTEGRATION REQUIREMENTS

## 12.1. Third-party Shipping Provider

The system interacts with a third-party shipping provider.

In the initial version:

- Book Shop hands the order over to the third party.
- The third party performs the shipping.
- The Customer pays upon delivery.
- Book Shop receives delivery-status information.
- An Employee confirms successful delivery in the system.

Book Shop **does not manage shipping operations** within the scope of the initial version.

## 12.2. Payment Integration

An online payment gateway is not required in Version 1.0.

Payment method:

**COD**

## 12.3. Notification Integration

The system supports sending notifications by email.

## 12.4. Chatbot

The chatbot is a component that helps Customers to:

- Search for books.
- Get book recommendations.
- Receive answers to product-related questions.

---

# 13. REPORTING REQUIREMENTS

## 13.1. Revenue Report

Filters:

- Date.
- Store.
- Product.
- Category.

Time periods:

- Day.
- Week.
- Month.
- Quarter.
- Year.

## 13.2. Order Report

Metrics:

- New Orders.
- Processing Orders.
- Completed Orders.
- Cancelled Orders.

## 13.3. Inventory Report

Metrics:

- Low-stock Books.
- Out-of-stock Books.
- Best-selling Books.
- Slow-moving Books.

## 13.4. Customer Report

Metrics:

- New Customers.
- Returning Customers.
- Purchase Frequency.

---

# 14. ACCEPTANCE CRITERIA

The system is considered to satisfy this SRS when the following core requirements pass testing:

### AC-001 — Search

Customers and Guests can search for books.

### AC-002 — Product Details

Users can view complete book information.

### AC-003 — Shopping Cart

Customers can:

- Add books.
- Change quantities.
- Remove books.
- View the total amount.

### AC-004 — Order

Customers can successfully create an order.

### AC-005 — Inventory Validation

The system does not allow orders that exceed the quantity available for sale.

### AC-006 — Payment

Customers can select COD.

### AC-007 — Order Tracking

Customers can track order statuses.

### AC-008 — Order Cancellation

Customers cannot cancel an order while it is being shipped.

### AC-009 — Product Management

Administrators can manage products.

### AC-010 — Order Management

Authorized Employees can process orders.

### AC-011 — Inventory Management

Book Shop can track inventory.

### AC-012 — Authorization

Users cannot access features outside their permissions.

### AC-013 — Reporting

Management can view the required reports.

### AC-014 — Reviews

Customers can submit reviews in accordance with the applicable policy.

### AC-015 — Promotions

The system does not apply expired promotions.

### AC-016 — Returns and Refunds

The system supports the return-request and refund process in accordance with policy.

These criteria are derived from the acceptance criteria in the BRD.

---

# 15. TRACEABILITY MATRIX

| BRD Requirement    | SRS Requirement               | Module        |
| ------------------ | ----------------------------- | ------------- |
| Account Management | FR-ACC-001 → FR-ACC-007       | Account       |
| Book Management    | FR-PROD-001 → FR-PROD-005     | Product       |
| Search             | FR-SEARCH-001 → FR-SEARCH-003 | Search        |
| Book Details       | FR-PROD-006 → FR-PROD-008     | Product       |
| Shopping Cart      | FR-CART-001 → FR-CART-005     | Cart          |
| Order Placement    | FR-ORDER-001 → FR-ORDER-005   | Order         |
| Payment            | FR-PAY-001 → FR-PAY-003       | Payment       |
| Order Management   | FR-ORDER-006 → FR-ORDER-011   | Order         |
| Returns            | FR-RETURN-001 → FR-RETURN-006 | Return/Refund |
| Inventory          | FR-INV-001 → FR-INV-003       | Inventory     |
| Stores             | FR-STORE-001 → FR-STORE-007   | Store         |
| Promotions         | FR-PROMO-001 → FR-PROMO-007   | Promotion     |
| Reviews            | FR-REVIEW-001 → FR-REVIEW-004 | Review        |
| Notifications      | FR-NOTI-001                   | Notification  |
| Chatbot            | FR-CHAT-001 → FR-CHAT-004     | Chatbot       |
| Reporting          | FR-REPORT-001 → FR-REPORT-004 | Reporting     |

---

# 16. OPEN ISSUES AND CLARIFICATIONS

The BRD has defined the business scope in considerable detail. However, the following issues must be clarified before proceeding to technical design.

## 16.1. Chatbot

The following points need to be determined:

- Does the chatbot only answer questions about books, or can it also access order information?
- Can the chatbot perform order-placement operations?
- Is conversation history stored?
- Can conversations be transferred to a customer support employee?

---

# 17. OUT OF SCOPE

The following features are not included in Version 1.0:

1. Third-party marketplace.
2. Livestream shopping.
3. Book rental.
4. Advanced ERP/accounting.
5. A dedicated mobile application.
6. Internal shipping management.
7. Online bank payments.
8. Other features not defined in the BRD.

These features may be considered in subsequent phases.

---

# 18. APPENDICES

## 18.1. Glossary

| Term                          | Definition                            |
| ----------------------------- | ------------------------------------- |
| BRD                           | Business Requirements Document        |
| SRS                           | Software Requirements Specification   |
| Guest                         | A user who is not signed in           |
| Customer                      | A customer who has an account         |
| Employee                      | A Book Shop employee                  |
| Store Manager                 | A store manager                       |
| Administrator                 | An administrator                      |
| Product/Book                  | A book product                        |
| Inventory                     | Stock on hand                         |
| Available Quantity            | Quantity available for sale           |
| Reserved Quantity             | Quantity reserved for an order        |
| Promotion                     | A promotional campaign                |
| COD                           | Cash on Delivery                      |
| Review                        | A product review                      |
| Order                         | A customer order                      |
| Return                        | Return of a product                   |
| Refund                        | Repayment of money                    |
| Third-party Shipping Provider | An external shipping service provider |

## 18.2. Requirement ID Convention

Requirement IDs follow the conventions below:

```text
FR-ACC      Account
FR-PROD     Product
FR-SEARCH   Search
FR-CART     Cart
FR-ORDER    Order
FR-PAY      Payment
FR-RETURN   Return/Refund
FR-INV      Inventory
FR-STORE    Store
FR-PROMO    Promotion
FR-REVIEW   Review
FR-NOTI     Notification
FR-CHAT     Chatbot
FR-REPORT   Reporting
NFR         Non-functional Requirement
BR          Business Rule
AC          Acceptance Criteria
```

## 18.3. Requirement Priority

The following convention may be used:

| Priority    | Meaning                                       |
| ----------- | --------------------------------------------- |
| Must Have   | Required for the system to operate            |
| Should Have | Important, but may be postponed               |
| Could Have  | May be implemented if resources are available |
| Won't Have  | Not included in the current version           |

For Version 1.0, core features such as:

- Product.
- Search.
- Cart.
- Order.
- COD.
- Inventory.
- Order Tracking.
- Authorization.
- Reporting.

are considered the main functional areas based on the BRD acceptance criteria.

---

# DOCUMENT CONTROL

| Item          | Value                               |
| ------------- | ----------------------------------- |
| Document      | Software Requirements Specification |
| Project       | Book Shop                           |
| Version       | 1.0                                 |
| Status        | Draft for Review                    |
| Based On      | Business Requirements Document v1.0 |
| Owner         | Founder & CEO — Book Shop           |
| Prepared By   | Business Analysis                   |
| Reviewers     | TBD                                 |
| Approval Date | TBD                                 |

---

# CONCLUSION

This SRS translates Book Shop's business requirements into functional requirements, non-functional requirements, business rules, access permissions, business processes, data requirements, acceptance criteria, and traceability.

This SRS **does not replace architecture or technical design documents** for defining topics such as:

- Technologies.
- Database engine.
- System architecture.
- API endpoints.
- Frameworks.
- Infrastructure.
- Deployment.
- Detailed technical security.

These topics will be defined in the corresponding technical documents during the next phase.

**End of Document**
