# ADR-001: CloudOpsHub Architecture

## Status

Accepted

## Context

CloudOpsHub is designed as a production-style cloud-native platform
for demonstrating modern Platform Engineering, DevOps, DevSecOps,
observability and SRE practices.

## Decision

The platform will use independently deployable services built with
ASP.NET Core and deployed as containers.

The initial services are:

- Order Service
- User Service
- Notification Service

PostgreSQL will provide persistent storage and RabbitMQ will provide
asynchronous messaging.

The platform will eventually run on Azure Kubernetes Service (AKS).

Infrastructure will be provisioned using Terraform.

Kubernetes applications will be packaged using Helm and deployed
using GitOps with ArgoCD.

## Consequences

This architecture provides opportunities to demonstrate:

- Microservices
- Containerization
- Kubernetes
- Infrastructure as Code
- GitOps
- CI/CD
- Distributed tracing
- Event-driven architecture
- Security
- Reliability engineering