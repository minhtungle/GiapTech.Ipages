# SOURCE CONTROL & REPOSITORY REQUIREMENTS

## MONOREPO STRATEGY

Toàn bộ hệ thống phải được quản lý trong một Git Repository duy nhất.

Không sử dụng:

```text
Git Submodule
Multiple Repository
External Source Dependency Repository
```

Cấu trúc:

```text
GiapTech.Ipages

├── backend
├── frontend-host
├── frontend-tenant
├── frontend-client
├── docker
├── database
├── docs
├── scripts
├── .gitignore
├── .env.example
├── docker-compose.yml
├── docker-compose.override.yml
└── README.md
```

---

# GITIGNORE REQUIREMENTS

Bắt buộc sinh file:

```text
.gitignore
```

tại root project.

Mục tiêu:

* Commit toàn bộ source code.
* Commit toàn bộ migration.
* Commit toàn bộ docker configuration.
* Commit toàn bộ infrastructure configuration.
* Không commit file build.
* Không commit runtime data.
* Không commit secrets.

---

# MUST COMMIT

Các file sau bắt buộc được đưa vào Git:

```text
Source Code

Dockerfile

docker-compose.yml

docker-compose.override.yml

.env.example

Migration Files

Seed Data

Traefik Configuration

HealthCheck Configuration

Scripts

README.md

Tests

Swagger Configuration

Database Initialization Scripts
```

---

# MUST IGNORE

## Backend

```text
**/bin/
**/obj/
**/.vs/
**/*.user
**/*.suo
```

---

## Frontend

```text
**/node_modules/
**/dist/
**/.vite/
**/.cache/
```

---

## Runtime

```text
logs/
temp/
tmp/
cache/
uploads/
storage/runtime/
```

---

## Docker Data

```text
postgres-data/
redis-data/
minio-data/
docker-data/
```

---

## Secrets

```text
.env

.env.local

.env.production

.env.development

appsettings.Local.json

appsettings.Production.json
```

---

# ENVIRONMENT MANAGEMENT

Bắt buộc sinh:

```text
.env.example
```

Chứa toàn bộ biến môi trường cần thiết.

Ví dụ:

```env
POSTGRES_DB=
POSTGRES_USER=
POSTGRES_PASSWORD=

JWT_SECRET=

MINIO_ACCESS_KEY=
MINIO_SECRET_KEY=

REDIS_PASSWORD=
```

Không được commit:

```text
.env
```

thực tế.

---

# INFRASTRUCTURE AS CODE

Mọi thành phần hạ tầng phải được quản lý bằng source code.

Bao gồm:

```text
Docker

Traefik

Database Migration

Seed Data

Storage Configuration

Health Checks

Startup Scripts
```

Không được yêu cầu người dùng tạo thủ công file ngoài repository.

---

# README REQUIREMENTS

README phải mô tả đầy đủ:

```text
Project Structure

Architecture

Environment Variables

Default Accounts

Docker Startup

Development Workflow

Production Deployment

Backup Strategy

Restore Strategy
```

---

# REPOSITORY ACCEPTANCE CRITERIA

Máy mới hoàn toàn phải có khả năng:

```bash
git clone <repository>

cd GiapTech.Ipages

cp .env.example .env

docker compose up -d
```

và toàn bộ hệ thống hoạt động.

Không cần cài đặt thủ công.

Không cần tải file bổ sung.

Không cần cấu hình ngoài repository.

---

# FINAL ACCEPTANCE CRITERIA

Dự án chỉ được xem là hoàn thành khi:

```bash
git clone <repository>

cp .env.example .env

docker compose up -d
```

thành công và có thể truy cập:

```text
http://host.localhost

http://host.localhost/admin

http://demo.localhost

http://demo.localhost/admin
```

với dữ liệu seed mặc định.

Mọi migration, seed data, docker configuration, source code, frontend, backend, storage configuration đều nằm trong repository duy nhất.
