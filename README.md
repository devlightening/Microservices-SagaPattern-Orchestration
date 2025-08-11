## 🚀 Microservices - Saga Pattern (Orchestration)

This project demonstrates an **orchestration-based Saga Pattern** implementation using **MassTransit** and **RabbitMQ** to ensure transactional consistency across multiple microservices. The architecture revolves around an order processing flow involving three core services: **OrderAPI**, **StockAPI**, and **PaymentAPI**.

-----

## 🎯 Project Goal

The goal is to **reliably manage a complex business process**—from order creation to payment completion—as a **distributed transaction** across multiple services. It uses **compensation (rollback)** mechanisms to maintain system integrity in case of failures.

-----

## 🏗️ Architecture

The system consists of multiple **loosely coupled microservices** communicating asynchronously via **RabbitMQ**.

  * **Orchestration:** `SagaStateMachineService` acts as the central orchestrator (state machine) managing the order lifecycle.
  * **Communication:** Services exchange events and commands. The `Shared` library contains all message contracts and RabbitMQ queue names.

-----

## 📊 Saga State Machine Diagram

This diagram illustrates the state transitions and message flow orchestrated by the `SagaStateMachineService`.

```mermaid
flowchart TD
 subgraph Services["Services"]
        OrderAPI["OrderAPI"]
        StockAPI["StockAPI"]
        PaymentAPI["PaymentAPI"]
  end
 subgraph SagaStateMachineService["SagaStateMachineService"]
        OrderCreated["OrderCreated"]
        StockReserved["StockReserved"]
        StockNotReserved["StockNotReserved"]
        PaymentCompleted["PaymentCompleted"]
        PaymentFailed["PaymentFailed"]
        StockRolledBack["StockRolledBack"]
  end
    OrderAPI -- OrderStartedEvent --> OrderCreated
    OrderCreated -- OrderCreatedEvent --> StockAPI & StockAPI
    StockAPI -- StockReservedEvent --> StockReserved
    StockAPI -- StockNotReservedEvent --> StockNotReserved
    StockReserved -- PaymentStartedEvent --> PaymentAPI & PaymentAPI
    PaymentAPI -- PaymentCompletedEvent --> PaymentCompleted
    PaymentAPI -- PaymentFailedEvent --> PaymentFailed
    StockNotReserved --> OrderAPI
    OrderAPI -- OrderFailedEvent --> OrderAPI & OrderAPI
    PaymentFailed --> StockAPI & OrderAPI
    StockAPI -- StockRollbackMessage --> StockAPI

    style OrderCreated fill:#ccffcc,stroke:#333,stroke-width:2px
    style StockReserved fill:#ccffcc,stroke:#333,stroke-width:2px
    style StockNotReserved fill:#ffcccc,stroke:#333,stroke-width:2px
    style PaymentCompleted fill:#ccffcc,stroke:#333,stroke-width:2px
    style PaymentFailed fill:#ffcccc,stroke:#333,stroke-width:2px,color:#000000
    style StockRolledBack fill:#ffcccc,stroke:#333,stroke-width:2px




```

-----

### Flow Description

1.  An `OrderStartedEvent` from `OrderAPI` initiates the saga, transitioning the state to **OrderCreated**.
2.  The `SagaStateMachineService` sends an `OrderCreatedEvent` to `StockAPI`.
3.  If stock is available, `StockAPI` sends back a `StockReservedEvent`, and the saga state changes to **StockReserved**. If not, `StockAPI` sends `StockNotReservedEvent`, the saga state changes to **StockNotReserved**, and a final `OrderFailedEvent` is sent to `OrderAPI`.
4.  When stock is reserved, a `PaymentStartedEvent` is sent to `PaymentAPI`.
5.  If payment succeeds, `PaymentAPI` sends `PaymentCompletedEvent`, the saga state becomes **PaymentCompleted**, and a final `OrderCompletedEvent` is sent to `OrderAPI`.
6.  If payment fails, `PaymentAPI` sends `PaymentFailedEvent`, the saga state becomes **PaymentFailed**, which triggers a **rollback**: a `StockRollbackMessage` is sent to `StockAPI` to revert the stock, and a final `OrderFailedEvent` is sent to `OrderAPI`.

-----

## 📂 Project Structure

### **`SagaStateMachineService`**

  * **Purpose:** Central orchestrator handling distributed transactions.
  * **DB:** MSSQL for saga persistence.
  * **Main Files:**
      * `OrderStateMachine.cs` → State transitions
      * `OrderStateInstance.cs` → Saga instance data
      * `OrderStateMap.cs` → DB mapping

### **`OrderAPI`**

  * **Purpose:** REST API to create/manage orders.
  * **DB:** MSSQL
  * **Main Files:**
      * `OrderCompletedEventConsumer.cs`
      * `OrderFailedEventConsumer.cs`
      * `OrderAPIDbContext.cs`

### **`StockAPI`**

  * **Purpose:** Manages product inventory.
  * **DB:** MongoDB
  * **Main Files:**
      * `OrderCreatedEventConsumer.cs`
      * `StockRollbackMessageConsumer.cs`
      * `MongoDbService.cs`

### **`PaymentAPI`**

  * **Purpose:** Handles payment simulation.
  * **Main Files:**
      * `PaymentStartedEventConsumer.cs`

### **`Shared`**

  * **Purpose:** Common contracts & settings.
  * **Main Files:**
      * Event definitions
      * Command definitions
      * `RabbitMQSettings.cs`

-----

## ⚙️ Setup & Run

1.  Start **RabbitMQ**, **MSSQL**, and **MongoDB**.
2.  Update connection strings in `appsettings.json`.
3.  Run DB migrations in `SagaStateMachineService`.
4.  Start services in order:
    1.  `SagaStateMachineService`
    2.  `OrderAPI`
    3.  `StockAPI`
    4.  `PaymentAPI`
5.  Use `OrderAPI` endpoints to create orders and trigger the saga flow.
