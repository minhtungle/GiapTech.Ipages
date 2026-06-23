# GIAPTECH.IPAGES

## ROLE

Bạn là Principal SaaS Architect, Principal Solution Architect, Senior ASP.NET Core Developer, Senior React Developer, Senior DevOps Engineer.

Nhiệm vụ của bạn là thiết kế và triển khai hoàn chỉnh một nền tảng SaaS Multi-Tenant Website Builder + Headless Commerce API có thể build và chạy thực tế.

Không tạo demo.

Không tạo pseudo code.

Không để TODO.

Không bỏ trống implementation.

Mọi module phải có source code hoàn chỉnh.

Mọi service phải chạy được bằng Docker.

Mục tiêu cuối cùng là:

```bash
docker compose up -d
```

toàn bộ hệ thống hoạt động ngay.

---

# PROJECT INFORMATION

## Project Name

```text
GiapTech.Ipages
```

## Root Folder

```text
E:\GIAPTECH\GiapTech.Ipages
```

---

# BUSINESS GOAL

Xây dựng nền tảng SaaS Multi-Tenant cho phép:

* Host tạo tenant.
* Tenant quản lý website riêng.
* Tenant quản lý landing page.
* Tenant quản lý sản phẩm.
* Tenant quản lý khách hàng.
* Tenant quản lý đơn hàng.
* Tenant quản lý bài viết.
* Tenant cung cấp API riêng.
* Website client side sử dụng API của tenant.
* Landing page sử dụng API của tenant.
* Có khả năng mở rộng thành SaaS thương mại.

Mô hình tương tự:

```text
Shopify
Haravan
WordPress MultiSite
Strapi Multi Tenant
```

---

# DOMAIN ARCHITECTURE

## Host Admin

```text
host.ipages.io.vn/admin
```

Vai trò:

```text
System Owner
```

---

## Tenant Admin

Ví dụ:

```text
abc.ipages.io.vn/admin

xyz.ipages.io.vn/admin
```

Trong đó:

```text
abc
xyz
```

là slug tenant.

---

## Client Website

Ví dụ:

```text
abc.ipages.io.vn

xyz.ipages.io.vn
```

Trang công khai dành cho khách hàng cuối.

---

# MULTI TENANT REQUIREMENTS

Mọi dữ liệu phải tách biệt.

Toàn bộ bảng nghiệp vụ phải có:

```text
TenantId
```

Áp dụng:

```text
Global Tenant Filter
```

Tenant không được phép truy cập dữ liệu tenant khác.

---

# TECHNOLOGY STACK

## Backend

```text
ASP.NET Core 9

Clean Architecture

CQRS

MediatR

Dapper

FluentValidation

JWT Authentication

Refresh Token

AutoMapper

Serilog

HealthChecks
```

---

## Frontend

```text
React

TypeScript

Vite

Ant Design

TanStack Query

TanStack Table

React Router

Axios

Zustand
```

---

## Database

```text
PostgreSQL
```

Không sử dụng SQL Server.

---

## Cache

```text
Redis
```

---

## Storage

Provider Pattern.

Mặc định:

```text
MinIO
```

---

## Reverse Proxy

```text
Traefik
```

---

## Deployment

```text
Docker

Docker Compose
```

---

# SOLUTION STRUCTURE

```text
GiapTech.Ipages

├── backend

│   ├── src

│   │   ├── GiapTech.Ipages.Api
│   │   ├── GiapTech.Ipages.Application
│   │   ├── GiapTech.Ipages.Domain
│   │   ├── GiapTech.Ipages.Infrastructure

│   └── tests

├── frontend-host

├── frontend-tenant

├── frontend-client

├── docker

├── storage

├── database

└── docs
```

---

# APPLICATIONS

## 1. Host Admin

Tên:

```text
GiapTech.Ipages.Host
```

Dành cho chủ hệ thống.

---

## 2. Tenant Admin

Tên:

```text
GiapTech.Ipages.Admin
```

Dành cho khách hàng quản trị website.

---

## 3. Headless API

Tên:

```text
GiapTech.Ipages.Api
```

Cung cấp dữ liệu cho:

* Admin
* Landing Page
* Website
* Mobile App

---

## 4. Client Website

Tên:

```text
GiapTech.Ipages.Client
```

Website công khai.

---

# USER TYPES

## Host Admin

Quản lý toàn hệ thống.

---

## Tenant Admin

Quản lý website tenant.

---

## Tenant Staff

Nhân viên tenant.

---

## Customer

Khách hàng cuối.

---

# HOST ADMIN MODULE

## Dashboard

Hiển thị:

```text
Tổng tenant

Tenant hoạt động

Doanh thu

API Calls

Storage Usage
```

---

## Tenant Management

```text
Tạo tenant

Sửa tenant

Khóa tenant

Kích hoạt tenant

Gia hạn tenant

Xóa tenant
```

---

## Subscription Management

```text
Gói dịch vụ

Ngày hết hạn

Gia hạn

Nâng cấp
```

---

## Payment History

```text
Lịch sử thanh toán tenant
```

---

## API Provider Management

```text
API Packages

External APIs

Rate Limits
```

---

# TENANT ADMIN MODULE

## Dashboard

Hiển thị:

```text
Doanh thu

Đơn hàng

Khách hàng

Sản phẩm

Lượt truy cập
```

---

## Organization Settings

```text
Tên đơn vị

Logo

Favicon

Email

Hotline

Địa chỉ

Mô tả

Social Links
```

---

## User Management

RBAC động.

Vai trò:

```text
Admin

Manager

Editor

Sales

Staff
```

---

## News Management

```text
Danh mục

Bài viết

Tag

SEO

Publish

Schedule Publish
```

---

## Product Management

### Category

```text
Danh mục cha

Danh mục con
```

---

### Product

```text
Tên

SKU

Slug

Mô tả

Giá

Giá giảm

Tồn kho

Ảnh

Trạng thái
```

---

### Product Variants

Ví dụ:

```text
Áo đỏ XL

Áo đỏ L

Áo xanh XL
```

---

### Product Attributes

```text
Màu sắc

Kích thước

Dung lượng
```

---

## Inventory

```text
Nhập kho

Xuất kho

Điều chỉnh tồn kho
```

---

## Customer Management

```text
Khách hàng

Địa chỉ

Lịch sử mua

Điểm tích lũy
```

---

## Order Management

### Order

```text
Mã đơn

Khách hàng

Sản phẩm

Tổng tiền

Trạng thái
```

---

### Order Status

```text
Pending

Confirmed

Shipping

Completed

Cancelled
```

---

## Payment Management

Hiện tại:

```text
COD

Bank Transfer
```

Thiết kế abstraction để hỗ trợ:

```text
VNPay

MoMo

ZaloPay
```

---

## Media Library

```text
Images

Videos

Documents
```

Có:

```text
Folder

Tag

Search
```

---

## Menu Builder

```text
Header Menu

Footer Menu

Sidebar Menu
```

---

## SEO Management

Mọi đối tượng hỗ trợ:

```text
Meta Title

Meta Description

Canonical URL

OG Tags
```

---

# LANDING PAGE BUILDER

## Landing Page

```text
Tên

Slug

Template

Status
```

---

## Sections

```text
Hero

Banner

Feature

Gallery

Product List

Article List

Contact Form

Custom HTML
```

---

## Builder

Drag And Drop Builder.

Tương tự:

```text
Elementor

Shopify Builder
```

---

# FORM BUILDER

Hỗ trợ:

```text
Contact Form

Lead Form

Survey Form
```

---

# API MANAGEMENT

Tenant được tạo API riêng.

---

## API Key

```text
Name

Key

Secret

Status
```

---

## Permission

```text
Products.Read

Products.Write

Orders.Read

Orders.Write

Articles.Read

Articles.Write
```

---

# HEADLESS API

Ví dụ:

```text
/api/products

/api/categories

/api/articles

/api/orders

/api/cart

/api/customers
```

Theo tenant:

```text
abc.ipages.io.vn/api/products
```

---

# SHOPPING CART

```text
Cart

Cart Item

Voucher

Discount
```

---

# AUDIT LOG

Ghi nhận:

```text
User

Action

Time

IP

Entity

Old Value

New Value
```

---

# UI/UX REQUIREMENTS

## Design Philosophy

Theo phong cách:

```text
Notion

Linear

Shopify Admin
```

---

## Layout

```text
Sidebar

Topbar

Main Workspace
```

---

## Sidebar

```text
Collapse

Expand

Remember State
```

---

## Modal First

Toàn bộ:

```text
Create

Update

Delete

Import
```

sử dụng:

```text
Modal

Drawer
```

Không chuyển trang thừa.

---

## Table

Mọi danh sách sử dụng:

```text
TanStack Table
```

Hỗ trợ:

```text
Search

Filter

Sort

Pagination

Column Visibility

Export Excel
```

---

# DATABASE TABLES

Bắt buộc migration đầy đủ.

```text
Tenants

Subscriptions

Payments

Users

Roles

Permissions

UserRoles

RolePermissions

Articles

ArticleCategories

Tags

Products

ProductCategories

ProductVariants

ProductAttributes

Inventories

Customers

CustomerAddresses

Orders

OrderItems

Cart

CartItems

Coupons

ApiKeys

LandingPages

LandingSections

Forms

FormEntries

MediaFiles

Menus

MenuItems

SeoMetadata

AuditLogs
```

---

# DOCKER FIRST REQUIREMENT

Triết lý:

```text
Clone Source

docker compose up -d

Hoàn tất
```

Không yêu cầu cài đặt thủ công.

---

# REQUIRED CONTAINERS

```text
traefik

frontend-host

frontend-tenant

frontend-client

backend-api

postgres

redis

minio
```

---

# ROUTING

```text
host.localhost

demo.localhost

abc.localhost

xyz.localhost
```

Tự động route bằng Traefik.

---

# AUTOMATIC STARTUP

Sau:

```bash
docker compose up -d
```

Hệ thống phải tự động:

1. Chờ PostgreSQL.
2. Chạy Migration.
3. Seed Data.
4. Seed Host Admin.
5. Seed Tenant Demo.
6. Seed Roles.
7. Seed Permissions.
8. Khởi động API.
9. Khởi động Frontend.
10. Khởi động Traefik.

Không yêu cầu thao tác thủ công.

---

# DEFAULT ACCOUNTS

## Host Admin

```text
Username: hostadmin

Password: Host@123456
```

---

## Tenant Admin

```text
Tenant: demo

Username: admin

Password: Admin@123456
```

---

# DOCKER STRUCTURE

```text
docker

├── traefik

├── postgres

├── redis

├── minio

├── scripts

└── init
```

---

# REQUIRED FILES

Bắt buộc tạo:

```text
backend/Dockerfile

frontend-host/Dockerfile

frontend-tenant/Dockerfile

frontend-client/Dockerfile

docker-compose.yml

docker-compose.override.yml

.env.example

README.md
```

---

# HEALTH CHECK

Mọi service phải có:

```text
/health
```

Docker Compose phải sử dụng healthcheck.

---

# ENVIRONMENT VARIABLES

```env
POSTGRES_PASSWORD=

JWT_SECRET=

MINIO_ACCESS_KEY=

MINIO_SECRET_KEY=
```

---

# SEED DATA

Tự động tạo:

```text
Host Admin

Demo Tenant

Roles

Permissions

Categories

Products mẫu
```

---

# NON FUNCTIONAL REQUIREMENTS

Bắt buộc:

```text
Multi Tenant

Clean Architecture

SOLID

CQRS

Repository Pattern

Dependency Injection

Caching

Redis Ready

API Versioning

Swagger

Global Exception Middleware

Audit Logging

Rate Limiting

Unit Test Friendly

Docker Ready
```

---

# DELIVERABLE

Triển khai theo từng phase.

Mỗi phase phải:

1. Build thành công.
2. Chạy thành công.
3. Có migration.
4. Có seed data.
5. Có docker update.
6. Có hướng dẫn chạy.

Sau khi hoàn thành phase hiện tại phải dừng lại.

Chờ tôi trả lời:

```text
continue
```

mới được triển khai phase tiếp theo.

Không được tạo pseudo code.

Không được để TODO.

Không được bỏ implementation.

Tiêu chí nghiệm thu cuối cùng:

```bash
docker compose up -d
```

=> Toàn bộ hệ thống hoạt động ngay.
