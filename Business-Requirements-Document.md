# BUSINESS REQUIREMENTS DOCUMENT (BRD)

# BOOK SHOP — ONLINE BOOKSTORE PLATFORM

**Version:** 1.0\
**Status:** Draft for Review\
**Owner:** Founder & CEO — Book Shop

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Business Objectives](#2-business-objectives)
3. [Scope and Users](#3-scope-and-users)
4. [Products, Search, and Shopping Experience](#4-products-search-and-shopping-experience)
5. [Accounts, Cart, Orders, and Payment](#5-accounts-cart-orders-and-payment)
6. [Orders and Customer Support](#6-orders-and-customer-support)
7. [Inventory, Stores, and Promotions](#7-inventory-stores-and-promotions)
8. [Reviews and Chatbot](#8-reviews-and-chatbot)
9. [Business Rules, Reports, and Security](#9-business-rules-reports-and-security)
10. [Success and Acceptance Criteria](#10-success-and-acceptance-criteria)

## 1. PROJECT OVERVIEW

### 1.1. Background

Book Shop is a bookstore chain expanding from physical retail into online sales.

The Book Shop website is intended to create an online sales channel while helping Book Shop manage customers, books, orders, payments, deliveries, inventory, and business operations consistently.

### 1.2. Problem to Be Solved

Book Shop needs a platform that can:

- Reach customers beyond the areas served by its stores.
- Help customers find and purchase books conveniently.
- Reduce manual processing.
- Improve the accuracy of product and order tracking.
- Provide information to support management and decision-making.

### 1.3. Vision

Build an omnichannel bookstore platform that connects customers, the website, stores, warehouses, and Book Shop's business operations.

---

## 2. BUSINESS OBJECTIVES

The project aims to:

1. Increase revenue from online sales.
2. Expand customer reach.
3. Improve the book-shopping experience.
4. Manage products and inventory accurately.
5. Reduce order processing time.
6. Provide reports that support management.
7. Create a platform that can scale in the future.

## 3. SCOPE AND USERS

### 3.1. In Scope

The website and management system include:

- Account management.
- Book and category management.
- Book search and filtering.
- Shopping cart and order placement.
- Payment.
- Order management.
- Inventory management.
- Store management.
- Promotions.
- Product reviews.
- Notifications.
- Business reports.
- Customer support chatbot.

### 3.2. Users

**Customers:** Search for and purchase books, make payments, track orders, leave reviews, and manage their accounts.

**Guest visitors:** Browse, search for, and discover books, but cannot use features that require an account.

**Employees:** Confirm orders for shipment and confirm successful delivery (receive a notification from a third party and update the order status to indicate successful delivery to the customer).

**Store managers:** Manage products, orders, and business information for their assigned stores.

**Administrators:** Manage all operations permitted by Book Shop.

### 3.3. Out of Scope for the Initial Release

- A marketplace for third-party sellers.
- Livestream shopping.
- Book rentals.
- A specialized accounting/ERP system.
- A standalone mobile application.
- Shipment management (handled by a third party).
- Online payments through banks or other online payment methods.

These features may be considered in a later phase.

---

## 4. PRODUCTS, SEARCH, AND SHOPPING EXPERIENCE

### 4.1. Product Management

Book Shop needs to manage the following information:

- Book title.
- Author.
- Publisher.
- Genre/category.
- Description.
- Images.
- Sale price.

Administrators can add, view, edit, discontinue, and reactivate products.

### 4.2. Search and Discovery

Customers can search by:

- Book title.
- Author.
- Publisher.
- Genre/category.
- Related keywords.

They can filter by:

- Price.
- Genre/category.
- Publisher.
- Rating.
- Availability.

They can sort by:

- Price.
- Best-selling.
- Rating.
- Newest books.

### 4.3. Book Detail Page

The book page should clearly display:

- Book title.
- Author.
- Images.
- Price.
- Promotional price, if applicable.
- Description.
- Publisher.
- Reviews.
- Availability.

Customers can add a book to their cart or buy it immediately.

---

## 5. ACCOUNTS, CART, ORDERS, AND PAYMENT

### 5.1. Accounts

Customers can:

- Register.
- Sign in.
- Verify their account.
- Recover their password.
- Update personal information.
- Manage delivery addresses.
- View purchase history.

### 5.2. Shopping Cart

Customers can:

- Add books.
- Change quantities.
- Remove items.
- View the total amount.

Customers must not be able to order more than the quantity available for sale.

### 5.3. Order Placement

Basic flow:

```text
Find books
   ↓
Select books
   ↓
Add to cart (optional)
   ↓
Enter a delivery address
   ↓
Choose a payment method (version 1.0 supports cash on delivery only)
   ↓
Apply a promotion (optional)
   ↓
Confirm the order (order status is Pending)
   ↓
An employee hands the order to a third-party carrier (order status is Shipping)
   ↓
Delivery (in version 1, the customer pays the third-party carrier)
   ↓
Completion (Book Shop receives the payment from the third party and confirms the order as Completed)
```

## 6. ORDERS AND CUSTOMER SUPPORT

### 6.1. Order Management

Customers can:

- View orders.
- Track order status.
- View items and amounts.
- Cancel an order if cancellation is still allowed.

*Note:* An order cannot be cancelled once its status is **Shipping**.

Authorized employees can:

- Search for orders.
- Confirm orders.
- Process orders.
- Cancel orders according to policy.
- Handle return and refund requests.

### 6.2. Returns and Refunds

- If a customer has received an order (the order status is **Completed**) and wants to return it, they must provide supporting evidence and bank account information.

Process:

```text
Customer submits a request
       ↓
Book Shop reviews the request
       ↓
Approve / Reject
       ↓
Receive and inspect the items
       ↓
Issue a refund if eligible
       ↓
Update the order and inventory
```

### 6.3. Notifications

Customers receive notifications about important events, such as:

- Account registration.
- Order placement.
- Payment.
- Order confirmation.
- Delivery.
- Order cancellation.
- Refund.
- Promotions.

---

## 7. INVENTORY, STORES, AND PROMOTIONS

### 7.1. Inventory Management

Book Shop needs to know the quantity of each book at every store or warehouse.

The system must track:

- Quantity on hand.
- Quantity reserved for orders.
- Quantity available for sale.
- Damaged quantity.
- Quantity awaiting receipt.

Inventory is updated when:

- Stock is received.
- Items are sold.
- An order is placed.
- An order is cancelled.
- Items are returned.
- Stock is transferred.
- Damaged items are recorded.

**Principle:** Do not sell more than the quantity available for sale.

### 7.2. Multi-Store Management

Book Shop can:

- View inventory at each store.
- Perform stock counts.
- Receive and issue stock.
- Transfer stock between locations.
- Receive stock from another store.
- Track the history of changes.

### 7.3. Promotions

Promotions may include:

- Percentage discounts.
- Fixed-amount discounts.
- Discount codes.
- Free shipping.
- Flash sales.
- Category-based promotions.

Promotions must follow conditions for eligible products, order value, time period, and number of uses.

---

## 8. REVIEWS AND CHATBOT

### 8.1. Reviews

Customers can rate a book from 1 to 5 stars and write a review.

Book Shop may require customers to have purchased a product before reviewing it.

Administrators can moderate and hide reviews that violate policy.

### 8.2. Chatbot

The chatbot helps customers:

- Find books.
- Get book recommendations.
- Ask questions about products.

In the future, it may be expanded into a personalized book recommendation system.

---

## 9. BUSINESS RULES, REPORTS, AND SECURITY

### 9.1. Business Rules

| ID | Business rule |
|---|---|
| BR-001 | The sale price must not be less than zero. |
| BR-002 | Customers must not order more than the quantity available for sale. |
| BR-003 | Expired promotions cannot be applied. |
| BR-004 | A user cannot use more points than they currently have. |
| BR-005 | Reviews must comply with Book Shop's policies. |
| BR-006 | An order can only be cancelled when the applicable conditions are met. |
| BR-007 | Refunds must comply with the return policy. |
| BR-008 | Users may only perform tasks that match their responsibilities. |
| BR-009 | The total amount must be clear before an order is placed. |
| BR-010 | An order cannot be cancelled while its status is Shipping. |

### 9.2. Reports

Management needs information about:

**Revenue**

- By day, week, month, quarter, and year.
- By store.
- By product.
- By category.

**Orders**

- New orders.
- Orders in progress.
- Completed orders.
- Cancelled orders.

**Products**

- Books running low.
- Out-of-stock books.
- Best-selling books.
- Slow-moving books.

**Customers**

- New customers.
- Returning customers.
- Purchase frequency.

---

## 10. SUCCESS AND ACCEPTANCE CRITERIA

### 10.1. Acceptance Criteria

The project is accepted when:

- Customers can search for and view books.
- Customers can add books to their cart.
- Customers can place orders.
- Customers can pay using the methods supported by Book Shop.
- The system prevents sales beyond the quantity available for sale.
- Customers can track their orders.
- Book Shop can manage products.
- Book Shop can manage orders.
- Book Shop can track inventory.
- Users can only perform functions appropriate to their responsibilities.
- Management can view the required reports.
- No critical defects affecting business operations remain at the time of acceptance.

### 10.2. Expected Outcomes

When complete, Book Shop will have an online sales channel that meets its main business needs:

- Customers can easily find and purchase books.
- Book Shop can manage products, orders, and inventory.
- The purchasing process is clear and convenient.
- Management has the information needed to monitor business operations.
- The platform can continue to expand as business needs grow.

### 10.3. BRD Guiding Principle

The BRD defines **what Book Shop needs, who needs it, and why**.

The BRD does not go into detail about how the development team will build the system. Topics such as:

- Technologies used.
- Database.
- Software architecture.
- API design.
- Detailed interface design.
- Technical requirements.
- Technical security.
- System deployment and operations.

will be described in specialized documents in later stages, particularly the **SRS, Software Architecture Document, API Specification, and Technical Design**.

---

**Document Owner:** Founder & CEO — Book Shop\
**Document Type:** Business Requirements Document\
**Version:** 1.0\
**Status:** Draft for Review
