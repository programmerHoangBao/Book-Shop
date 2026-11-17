# Book-Shop – CI/CD Deployment Guide

This document provides instructions for setting up CI/CD for the **Book-Shop** project on an Ubuntu Server using the following technologies:

- **GitHub Actions**: Automatically validates and builds Docker images.
- **GitHub Container Registry (GHCR)**: Stores Docker images.
- **GitHub self-hosted runner**: Receives and executes deployment jobs on the Ubuntu Server.
- **Docker Compose**: Updates the backend without building Docker images directly on the server.

Repository: https://github.com/programmerHoangBao/Book-Shop

## Table of Contents

- [Book-Shop – CI/CD Deployment Guide](#book-shop--cicd-deployment-guide)
  - [Table of Contents](#table-of-contents)
  - [1. CI/CD Architecture](#1-cicd-architecture)
    - [Workflow Overview](#workflow-overview)
  - [2. Current System Components](#2-current-system-components)
  - [3. Modify Docker Compose to Use GHCR](#3-modify-docker-compose-to-use-ghcr)
    - [3.1. Replace `build` with `image`](#31-replace-build-with-image)
    - [3.2. Notes on `appsettings.Production.json`](#32-notes-on-appsettingsproductionjson)
    - [3.3. Update the `.env` File on Ubuntu Server](#33-update-the-env-file-on-ubuntu-server)
    - [3.4. Validate the Docker Compose Configuration](#34-validate-the-docker-compose-configuration)
  - [4. Prepare Ubuntu Server for the Self-Hosted Runner](#4-prepare-ubuntu-server-for-the-self-hosted-runner)
    - [4.1. Check Docker](#41-check-docker)
    - [4.2. Grant Docker Permissions to the Runner Account](#42-grant-docker-permissions-to-the-runner-account)
    - [4.3. Register a Self-Hosted Runner with GitHub](#43-register-a-self-hosted-runner-with-github)
  - [5. Configure GitHub Actions Variables](#5-configure-github-actions-variables)
  - [6. Create the GitHub Actions Workflow](#6-create-the-github-actions-workflow)
    - [6.1. Remove the `.github` Ignore Rule](#61-remove-the-github-ignore-rule)
    - [6.2. Create the Workflow File](#62-create-the-workflow-file)
    - [6.3. Workflow Explanation](#63-workflow-explanation)
    - [6.4. Create the `production` Environment](#64-create-the-production-environment)
  - [7. Commit and Trigger CI/CD](#7-commit-and-trigger-cicd)
  - [8. Verify the Deployment Status](#8-verify-the-deployment-status)
  - [9. Rollback in Case of Deployment Failure](#9-rollback-in-case-of-deployment-failure)
  - [10. Troubleshooting Common Issues](#10-troubleshooting-common-issues)
    - [10.1. Workflow Does Not Appear in the Actions Tab](#101-workflow-does-not-appear-in-the-actions-tab)
    - [10.2. The Deploy Job Remains in the Queued State](#102-the-deploy-job-remains-in-the-queued-state)
    - [10.3. The Runner Cannot Execute Docker Commands](#103-the-runner-cannot-execute-docker-commands)
    - [10.4. `denied` or `unauthorized` Error When Pulling the Image](#104-denied-or-unauthorized-error-when-pulling-the-image)
    - [10.5. The Backend Starts but Continuously Restarts](#105-the-backend-starts-but-continuously-restarts)
    - [10.6. Docker Compose Creates New Volumes or Cannot Find Existing Data](#106-docker-compose-creates-new-volumes-or-cannot-find-existing-data)
    - [10.7. The Database Schema Has Not Been Updated](#107-the-database-schema-has-not-been-updated)
  - [11. Security and Operational Considerations](#11-security-and-operational-considerations)
  - [12. References](#12-references)

## 1. CI/CD Architecture

```text
Developer
   |
   | git push / Pull Request
   v
GitHub Repository (main)
   |
   v
GitHub Actions - CI (GitHub-hosted runner)
   |
   |-- Pull Request: Build Docker image for validation, without pushing
   |
   `-- Push to main: Build and push image to GHCR
                         |
                         v
                  GitHub Container Registry
                         |
                         v
              GitHub Actions - CD
                 (self-hosted runner)
                         |
                         | docker compose pull
                         | docker compose up -d
                         v
                  Ubuntu Server
                         |
                         v
                 Book-Shop Backend
```

### Workflow Overview

| Event | CI | Push Image to GHCR | Deploy to Server |
|---|---|---|---|
| Pull Request to `main` | Yes | No | No |
| Push/merge to `main` | Yes | Yes | Yes |

Each image is tagged with two tags:

```text
ghcr.io/programmerhoangbao/book-shop-back-end:latest
ghcr.io/programmerhoangbao/book-shop-back-end:<COMMIT_SHA>
```

The `latest` tag represents the most recently built image. The commit SHA tag identifies the exact deployed version and supports rollback operations.

## 2. Current System Components

According to the Docker Compose configuration in the repository, the system consists of the following services:

| Service | Docker Image | Role |
|---|---|---|
| `back-end` | Project GHCR image | ASP.NET Core Web API (.NET 9) |
| `postgresql` | `postgres:17` | PostgreSQL database |
| `redis` | `redis:7-alpine` | Cache and refresh token storage |
| `kafka` | `apache/kafka:4.0.0` | Message broker |
| `elasticsearch` | `elasticsearch:9.5.6` | Book search engine |

The following data volumes must be preserved during deployment:

```text
kafka_data
postgresql_data
redis_data
elasticsearch_data
```

CI/CD only builds and replaces the backend image. These data volumes must not be deleted during the deployment process.

## 3. Modify Docker Compose to Use GHCR

Currently, the `back-end` service in `docker-compose.yml` uses `build` to build the image directly on the server. When switching to CI/CD, GitHub Actions will build the image and push it to GHCR. Therefore, the server only needs to pull the image.

### 3.1. Replace `build` with `image`

In `docker-compose.yml`, replace the beginning of the `back-end` service with the following configuration:

```yaml
  back-end:
    container_name: book-shop-back-end

    image: ghcr.io/programmerhoangbao/book-shop-back-end:${IMAGE_TAG:-latest}

    ports:
      - "7282:8081"
      - "5070:8080"

    environment:
      ASPNETCORE_ENVIRONMENT: Production

      AWS_ACCESS_KEY_ID: ${AWS_ACCESS_KEY_ID}
      AWS_SECRET_ACCESS_KEY: ${AWS_SECRET_ACCESS_KEY}

      AWS__S3__Region: ${REGION}
      AWS__S3__BucketName: ${BUCKET_NAME}

    volumes:
      - ${APPSETTINGS_PRODUCTION_PATH}:/app/appsettings.Production.json:ro

    depends_on:
      postgresql:
        condition: service_healthy
      redis:
        condition: service_healthy
      kafka:
        condition: service_started
      elasticsearch:
        condition: service_healthy

    restart: unless-stopped
```

Key changes:

- Replace `build.context` and `build.dockerfile` with `image`, pointing to the GHCR image.
- `IMAGE_TAG` receives the commit SHA from the workflow. When running manually without specifying this variable, Compose defaults to `latest`.
- Mount the `appsettings.Production.json` configuration file from the server into the container.

Keep the existing PostgreSQL, Redis, Kafka, Elasticsearch services, and their volumes unchanged in `docker-compose.yml`.

### 3.2. Notes on `appsettings.Production.json`

The repository currently ignores backend configuration files, including `back-end/appsettings.Production.json`. Therefore, this file will not be available in a clean checkout used by GitHub Actions to build the Docker image.

To ensure that the container receives the Production configuration after deployment, this guide mounts the existing configuration file from the Ubuntu Server:

```yaml
    volumes:
      - ${APPSETTINGS_PRODUCTION_PATH}:/app/appsettings.Production.json:ro
```

The configuration file must already exist on the server before deployment.

Never commit files containing passwords, JWT secrets, SMTP passwords, or AWS credentials to the repository.

In `appsettings.Production.json`, services should use their internal Docker Compose hostnames. For example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgresql;Port=5432;Database=<database>;Username=<username>;Password=<password>"
  },
  "Redis": {
    "ConnectionString": "redis:6379,password=<redis-password>"
  },
  "Kafka": {
    "BootstrapServers": "kafka:9092"
  },
  "Elasticsearch": {
    "Url": "http://elasticsearch:9200",
    "Username": "elastic",
    "Password": "<elasticsearch-password>",
    "DefaultIndex": "books"
  }
}
```

This is only an illustrative configuration for the services listed above.

Keep all actual configuration sections used by your application, including `Security`, `Email`, `Authentication:Google`, `AWS:S3`, and other required settings.

The `Elasticsearch` section is required by the backend's current Elasticsearch initialization configuration.

### 3.3. Update the `.env` File on Ubuntu Server

The `.env` file should be located in the deployment directory, for example `/home/bao/projects/Book-Shop/.env`:

```dotenv
# PostgreSQL
POSTGRES_DB=<database-name>
POSTGRES_USER=<postgres-username>
POSTGRES_PASSWORD=<strong-postgres-password>

# Redis
REDIS_PASSWORD=<strong-redis-password>

# Elasticsearch
ELASTIC_PASSWORD=<strong-elasticsearch-password>

# AWS S3
AWS_ACCESS_KEY_ID=<aws-access-key-id>
AWS_SECRET_ACCESS_KEY=<aws-secret-access-key>
REGION=<aws-region>
BUCKET_NAME=<s3-bucket-name>

# Backend Production configuration on the server
APPSETTINGS_PRODUCTION_PATH=/home/bao/projects/Book-Shop/back-end/appsettings.Production.json
```

Replace all placeholders with the actual configuration values.

`APPSETTINGS_PRODUCTION_PATH` must contain the absolute path to the Production configuration file on the server.

Ensure that `.env` remains excluded by Git. Never include the actual contents of this file in the README or commit it to GitHub.

### 3.4. Validate the Docker Compose Configuration

On the server, run:

```bash
cd ~/projects/Book-Shop
sudo docker compose --env-file .env config -q
```

If the command completes without errors, Docker Compose can parse the configuration using the current `.env` file.

Also verify that the Production configuration file exists:

```bash
ls -l "$HOME/projects/Book-Shop/back-end/appsettings.Production.json"
```

## 4. Prepare Ubuntu Server for the Self-Hosted Runner

The self-hosted runner must run under a Linux account that has permission to execute Docker commands.

### 4.1. Check Docker

```bash
sudo systemctl status docker
sudo docker version
sudo docker compose version
```

### 4.2. Grant Docker Permissions to the Runner Account

For example, if the Linux account running the runner is `bao`:

```bash
sudo usermod -aG docker bao
```

Log out and log back in, or restart the runner service after changing group membership.

Verify Docker access using the same account that runs the runner:

```bash
sudo -u bao docker ps
```

Avoid running the GitHub runner as `root`. Members of the `docker` group have highly privileged access to the host, so only grant this permission to trusted accounts.

### 4.3. Register a Self-Hosted Runner with GitHub

Navigate to the repository:

```text
https://github.com/programmerHoangBao/Book-Shop
```

Open:

```text
Settings
  → Actions
    → Runners
      → New self-hosted runner
```

Select the Linux operating system and the architecture matching your server, for example `x64`.

GitHub will display the commands required to download and configure the runner.

In the runner directory, execute the `config.sh` command provided by GitHub. It will look similar to:

```bash
./config.sh \
  --url https://github.com/programmerHoangBao/Book-Shop \
  --token <RUNNER_REGISTRATION_TOKEN>
```

The runner registration token is temporary. Generate a new token from Settings → Actions → Runners when configuring the runner.

Do not store this token in the repository.

When prompted for a runner group, you can press Enter to use the `Default` group if no custom group is required.

You can keep the default runner name or choose a recognizable name, such as `book-shop-server`.

Install the runner as a service so that it starts automatically with the server:

```bash
sudo ./svc.sh install bao
sudo ./svc.sh start
sudo ./svc.sh status
```

Verify the runner status at:

```text
Repository → Settings → Actions → Runners
```

The runner should display the `Idle` status or otherwise indicate that it is ready to receive jobs.

> If the runner has already been installed and is online on the server, there is no need to register another runner.

## 5. Configure GitHub Actions Variables

The workflow uses a repository variable to identify the deployment directory on the server.

Navigate to:

```text
Repository
  → Settings
    → Secrets and variables
      → Actions
        → Variables
          → New repository variable
```

Create the following variable:

| Name | Example Value |
|---|---|
| `DEPLOY_PATH` | `/home/bao/projects/Book-Shop` |

The value must point to the directory on the Ubuntu Server containing `.env` and `back-end/appsettings.Production.json`.

The workflow uses the automatically generated `GITHUB_TOKEN` provided by GitHub Actions to authenticate with GHCR.

You do not need to create a separate Personal Access Token for this image publishing and pulling workflow, provided that the package is properly linked to the repository and the required permissions are granted.

## 6. Create the GitHub Actions Workflow

### 6.1. Remove the `.github` Ignore Rule

The current repository `.gitignore` file contains the following line:

```gitignore
/.github
```

This rule causes Git to ignore the entire `.github` directory, including workflow files.

Remove the `/.github` line from `.gitignore` before adding the workflow.

Do not remove ignore rules for sensitive configuration files, such as:

```gitignore
/.env
/back-end/appsettings.Development.json
/back-end/appsettings.json
/back-end/appsettings.Production.json
```

### 6.2. Create the Workflow File

At the repository root, create the following file:

```text
.github/workflows/ci-cd.yml
```

Content:

```yaml
name: Book-Shop CI/CD

on:
  push:
    branches:
      - main
  pull_request:
    branches:
      - main

permissions:
  contents: read

jobs:
  build-and-push:
    name: Build Docker image
    runs-on: ubuntu-latest

    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout source code
        uses: actions/checkout@v6

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v4

      - name: Login to GitHub Container Registry
        if: github.event_name == 'push'
        uses: docker/login-action@v4
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push Docker image
        uses: docker/build-push-action@v7
        with:
          context: .
          file: ./back-end/Dockerfile
          push: ${{ github.event_name == 'push' }}
          tags: |
            ghcr.io/programmerhoangbao/book-shop-back-end:latest
            ghcr.io/programmerhoangbao/book-shop-back-end:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  deploy:
    name: Deploy to Ubuntu Server
    needs: build-and-push
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    runs-on: [self-hosted, Linux, X64]
    environment: production

    permissions:
      contents: read
      packages: read

    steps:
      - name: Checkout source code
        uses: actions/checkout@v6

      - name: Validate server configuration
        env:
          DEPLOY_PATH: ${{ vars.DEPLOY_PATH }}
        run: |
          set -euo pipefail

          if [ -z "$DEPLOY_PATH" ]; then
            echo "DEPLOY_PATH repository variable is not configured."
            exit 1
          fi

          test -d "$DEPLOY_PATH"
          test -f "$DEPLOY_PATH/.env"
          test -f "$DEPLOY_PATH/back-end/appsettings.Production.json"
          test -f "$GITHUB_WORKSPACE/docker-compose.yml"

      - name: Login to GitHub Container Registry
        env:
          GHCR_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          echo "$GHCR_TOKEN" | docker login ghcr.io \
            --username "${{ github.actor }}" \
            --password-stdin

      - name: Pull and deploy backend
        env:
          DEPLOY_PATH: ${{ vars.DEPLOY_PATH }}
          IMAGE_TAG: ${{ github.sha }}
        run: |
          set -euo pipefail

          cd "$DEPLOY_PATH"

          docker compose \
            --project-name book-shop \
            --project-directory "$DEPLOY_PATH" \
            --env-file "$DEPLOY_PATH/.env" \
            -f "$GITHUB_WORKSPACE/docker-compose.yml" \
            pull back-end

          docker compose \
            --project-name book-shop \
            --project-directory "$DEPLOY_PATH" \
            --env-file "$DEPLOY_PATH/.env" \
            -f "$GITHUB_WORKSPACE/docker-compose.yml" \
            up -d --remove-orphans

          docker compose \
            --project-name book-shop \
            --project-directory "$DEPLOY_PATH" \
            --env-file "$DEPLOY_PATH/.env" \
            -f "$GITHUB_WORKSPACE/docker-compose.yml" \
            ps

      - name: Logout from GitHub Container Registry
        if: always()
        run: docker logout ghcr.io || true
```

### 6.3. Workflow Explanation

**Job `build-and-push`** runs on GitHub's `ubuntu-latest` runner:

- Checks out the source code.
- Sets up Docker Buildx.
- For Pull Requests: builds the Docker image to validate the Dockerfile without pushing the image.
- For pushes to `main`: logs in to GHCR and pushes the image with both `latest` and commit SHA tags.

**Job `deploy`** only runs when code is pushed to `main`:

- Runs on the self-hosted runner installed on the Ubuntu Server.
- Validates the deployment directory, `.env`, and `appsettings.Production.json`.
- Logs in to GHCR using `GITHUB_TOKEN`.
- Pulls the image using the commit SHA tag.
- Runs `docker compose up -d` to update the system.
- Logs out of GHCR after deployment.

The image repository owner has been converted to lowercase, following Docker image naming conventions: `programmerhoangbao`.

The workflow uses `--project-name book-shop` to maintain a consistent Docker Compose project name.

If you previously ran Compose using a different project name, check the existing volume names before deployment and adjust this value to match the project name currently associated with those volumes.

Do not change the project name arbitrarily because Compose may create a new set of named volumes, making it appear as though the application has lost its data.

### 6.4. Create the `production` Environment

The workflow declares:

```yaml
environment: production
```

GitHub may automatically create this environment when the workflow runs for the first time.

You can navigate to:

```text
Settings → Environments → production
```

to configure protection rules, such as requiring approval before deploying to Production.

## 7. Commit and Trigger CI/CD

After updating `docker-compose.yml`, `.gitignore`, and creating the workflow, check the modified files:

```bash
git status --short
```

Make sure `.github/workflows/ci-cd.yml` appears in the list of files tracked by Git.

Then commit and push the changes:

```bash
git add .gitignore docker-compose.yml .github/workflows/ci-cd.yml
git commit -m "ci: configure GitHub Actions CI/CD deployment"
git push origin main
```

If you are committing this README at the same time, include `README.md` in the `git add` command.

Monitor the workflow at:

```text
GitHub Repository → Actions → Book-Shop CI/CD
```

When the workflow completes successfully:

1. The Docker image is published to the GitHub Packages tab.
2. The deployment job runs on the self-hosted runner.
3. The backend on the Ubuntu Server is updated using the image tagged with the commit SHA of that push.

## 8. Verify the Deployment Status

On the Ubuntu Server, run:

```bash
sudo docker ps
```

Check the status using Docker Compose:

```bash
cd ~/projects/Book-Shop
sudo docker compose --env-file .env ps
```

View the backend logs:

```bash
sudo docker logs -f book-shop-back-end
```

Alternatively:

```bash
sudo docker compose --env-file .env logs -f back-end
```

According to the current port mapping configuration:

| Component | Server Address/Port |
|---|---|
| Backend HTTP | `http://<SERVER_IP>:5070` |
| Backend HTTPS mapping | `https://<SERVER_IP>:7282` |
| PostgreSQL | `5432` |
| Redis | `6379` |
| Kafka external listener | `29092` |
| Elasticsearch | `9200` |

The HTTPS port will only work correctly if ASP.NET Core has been configured with a valid certificate and an appropriate HTTPS endpoint inside the container.

## 9. Rollback in Case of Deployment Failure

Since each image is tagged with its commit SHA, you can redeploy a previous version by setting `IMAGE_TAG` to the corresponding SHA.

For example, run the following commands using an account that has Docker permissions:

```bash
cd ~/projects/Book-Shop
export IMAGE_TAG=<PREVIOUS_COMMIT_SHA>
docker compose \
  --project-name book-shop \
  --env-file .env \
  up -d back-end
```

If you need to use `sudo`, explicitly pass the environment variable:

```bash
sudo env IMAGE_TAG=<PREVIOUS_COMMIT_SHA> docker compose \
  --project-name book-shop \
  --env-file .env \
  up -d back-end
```

Check the logs after rollback:

```bash
sudo docker logs --tail 200 book-shop-back-end
```

Do not delete Docker volumes during rollback.

## 10. Troubleshooting Common Issues

### 10.1. Workflow Does Not Appear in the Actions Tab

A possible cause is that `.github` is being ignored by Git.

Check `.gitignore` and remove the following line:

```gitignore
/.github
```

Then add the workflow file to Git and push the changes again.

### 10.2. The Deploy Job Remains in the Queued State

Check the following:

- The self-hosted runner is online.
- The runner has the labels `self-hosted`, `Linux`, and `X64`.
- The workflow uses the correct runner labels.
- The runner service is running:

```bash
sudo ./svc.sh status
```

### 10.3. The Runner Cannot Execute Docker Commands

Verify Docker access using the same account that runs the runner:

```bash
sudo -u bao docker ps
```

If you encounter a `permission denied` error, check Docker group membership and restart the runner service after adding the user to the group.

### 10.4. `denied` or `unauthorized` Error When Pulling the Image

Check the following:

- Is the image name correct? It should be `ghcr.io/programmerhoangbao/book-shop-back-end`.
- Does the deploy job have `packages: read` permission?
- Has the GHCR package been granted access to the current repository?
- Does the `docker login ghcr.io` command use the workflow's `GITHUB_TOKEN`?

If the package was previously created by manually pushing an image, check the Package settings and grant the package access to the repository.

### 10.5. The Backend Starts but Continuously Restarts

Check the logs:

```bash
sudo docker logs --tail 200 book-shop-back-end
```

Pay particular attention to the following:

- Is `appsettings.Production.json` mounted correctly?
- Does `ConnectionStrings:DefaultConnection` point to `postgresql` instead of `localhost`?
- Does the Redis connection string point to `redis`?
- Does Kafka's `BootstrapServers` point to `kafka:9092`?
- Does the Elasticsearch URL point to `http://elasticsearch:9200`?
- Is `Security:JwtSecretKey` configured?
- Are `AWS:S3` settings and AWS credentials valid?

### 10.6. Docker Compose Creates New Volumes or Cannot Find Existing Data

Check the volume names:

```bash
sudo docker volume ls
```

Check the Docker Compose project name currently in use.

The sample workflow sets the project name to `book-shop`. This value must match the project name originally used to create the existing volumes.

### 10.7. The Database Schema Has Not Been Updated

The sample workflow does not automatically execute `dotnet ef database update`.

This is intentional to prevent unexpected database schema changes in the Production environment.

When new migrations are introduced, perform a separate backup and migration process, then verify the application.

Do not delete the PostgreSQL volume to resolve migration errors.

## 11. Security and Operational Considerations

1. **Do not run Pull Request code from untrusted sources on the self-hosted runner.** This repository is public. The sample workflow only deploys from pushes to `main`, while Pull Request CI runs on a GitHub-hosted runner.
2. Protect the `main` branch, restrict who can push or merge changes, and consider requiring code reviews before Production deployment.
3. Never commit `.env`, `appsettings.Production.json`, AWS keys, SMTP passwords, JWT secrets, or database passwords.
4. The runner account requires Docker permissions, which effectively provide a very high level of administrative access to the server. Only grant these permissions to trusted accounts.
5. Avoid exposing PostgreSQL, Redis, Kafka, and Elasticsearch directly to the Internet unless external access is necessary. Containers within the same Docker network can communicate using service hostnames without publishing their ports to the host. For administrative access, prefer binding services to `127.0.0.1` and using an SSH tunnel or firewall.
6. Back up PostgreSQL and other important data before deploying major changes.
7. Never run `docker compose down -v` in Production because the `-v` option may delete named volumes containing important data.
8. Check service logs and health statuses after every deployment.

## 12. References

- [Book-Shop Repository](https://github.com/programmerHoangBao/Book-Shop)
- [GitHub Actions – Self-hosted runners](https://docs.github.com/en/actions/hosting-your-own-runners)
- [GitHub Actions – Secure use reference](https://docs.github.com/en/actions/reference/security/secure-use)
- [GitHub Packages – Working with the Container registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [Docker – Compose documentation](https://docs.docker.com/compose/)
- [Docker Build Push Action](https://github.com/docker/build-push-action)