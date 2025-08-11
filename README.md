-----

# 🚀 Microservices-SagaPattern-Orchestration

This project demonstrates an orchestration-based Saga Pattern implementation using MassTransit and RabbitMQ to ensure transactional consistency across multiple microservices. The architecture is built around an order processing flow involving three core services: `OrderAPI`, `StockAPI`, and `PaymentAPI`.

## 🎯 Project Goal

The primary goal of this project is to reliably manage a complex business process—from order creation to payment completion—as a distributed transaction spanning multiple services. It employs a robust compensation (rollback) mechanism to maintain system integrity in the event of failures, a critical aspect of distributed systems.

## 🏗️ Architecture

The system is composed of several decoupled microservices that communicate asynchronously via RabbitMQ message queues.

  * **Orchestration:** The `SagaStateMatchineService` project acts as a central orchestrator (a saga state machine) that governs the entire lifecycle of an order.
  * **Communication:** Services interact by sending and consuming events and commands. The `Shared` project centralizes the definitions of these messages and all RabbitMQ queue names.

### **Saga State Machine Diagram**

This diagram illustrates the state transitions and message flow for the order saga. Each state represents a specific stage of the order, and each message triggers a transition or an action.

```mermaid
stateDiagram-v2
    direction LR
    
    state "Initial" as Initial
    state "OrderCreated" as OrderCreated
    state "StockReserved" as StockReserved
    state "StockNotReserved" as StockNotReserved
    state "PaymentCompleted" as PaymentCompleted
    state "PaymentFailed" as PaymentFailed

    [*] --> Initial
    Initial --> OrderCreated : OrderStartedEvent
    
    OrderCreated --> StockReserved : StockReservedEvent
    OrderCreated --> StockNotReserved : StockNotReservedEvent
    
    StockReserved --> PaymentCompleted : PaymentCompletedEvent
    StockReserved --> PaymentFailed : PaymentFailedEvent
    
    StockNotReserved --> [*] : OrderFailedEvent
    PaymentCompleted --> [*] : OrderCompletedEvent
    PaymentFailed --> [*] : OrderFailedEvent & StockRollbackMessage
    
    state "Final" {
        [*] --> OrderCompletedEvent
        [*] --> OrderFailedEvent
    }

    classDef default fill:#f9f,stroke:#333,stroke-width:2px;
    classDef success fill:#ccffcc,stroke:#333,stroke-width:2px;
    classDef fail fill:#ffcccc,stroke:#333,stroke-width:2px;
    class OrderCreated,StockReserved,PaymentCompleted success;
    class StockNotReserved,PaymentFailed fail;

```

**Description of the Flow:**

1.  An `OrderStartedEvent` from the `OrderAPI` initiates the saga, transitioning the state to `OrderCreated`.
2.  The orchestrator then sends a message to the `StockAPI`.
3.  Based on the response (`StockReservedEvent` or `StockNotReservedEvent`), the saga either proceeds to `StockReserved` or terminates in `StockNotReserved`.
4.  If the stock is reserved, a message is sent to the `PaymentAPI`.
5.  A `PaymentCompletedEvent` leads to a successful `PaymentCompleted` state and finalizes the saga. A `PaymentFailedEvent` transitions the saga to `PaymentFailed` and triggers a compensation message (`StockRollbackMessage`) to the `StockAPI`.
6.  Both successful (`PaymentCompleted`) and failed (`StockNotReserved`, `PaymentFailed`) states lead to a final state, indicating the saga is complete.

## 📂 Project Structure

The repository is organized into the following projects, with each service utilizing a specific database technology:

### **`SagaStateMatchineService`**

  - ⚙️ **Purpose:** The central orchestrator that manages the distributed transaction flow.
  - 🗄️ **Database:** **MSSQL** is used to persist the state of the saga instances (`OrderStateInstance`).
  - 📌 **Key Components:**
      * `StateMachines/OrderStateMachine.cs`: Defines the state transitions and actions for the order saga.
      * `StateInstances/OrderStateInstance.cs`: The data model for the saga instance, persisted in the database.
      * `StateMaps/OrderStateMap.cs`: Configures how the saga state is mapped to the database.
      * `Migrations`: Contains database migration scripts to set up the saga persistence store.

### **`OrderAPI`**

  - 🛍️ **Purpose:** A RESTful API for creating and managing orders.
  - 🗄️ **Database:** **MSSQL** is used to store order-related data.
  - 📌 **Key Components:**
      * `Consumer/OrderCompletedEventConsumer.cs`: Handles messages for successful order completion.
      * `Consumer/OrderFailedEventConsumer.cs`: Handles messages for failed orders.
      * `Context/OrderAPIDbContext.cs`: The database context for order-related data.
      * `Entities/Order.cs`, `OrderItem.cs`: Models for the order database tables.

### **`StockAPI`**

  - 📦 **Purpose:** Manages the product inventory.
  - 🗄️ **Database:** **MongoDB** is used as a NoSQL database for flexible stock data management.
  - 📌 **Key Components:**
      * `Consumers/OrderCreatedEventConsumer.cs`: Consumes `OrderCreatedEvent` to reserve stock.
      * `Consumers/StockRollbackMessageConsumer.cs`: Consumes `StockRollbackMessage` to unreserve stock in case of payment failure.
      * `Entities/Stock.cs`: The model for the stock database.
      * `Services/MongoDbService.cs`: Manages communication with the MongoDB database.

### **`PaymentAPI`**

  - 💳 **Purpose:** A service that simulates payment processing.
  - 🗄️ **Database:** (No specific database is mentioned, but it would typically interact with a payment gateway or a database to log transactions).
  - 📌 **Key Components:**
      * `Consumers/PaymentStartedEventConsumer.cs`: Consumes `PaymentStartedEvent` to begin the payment process.

### **`Shared`**

  - 🤝 **Purpose:** A shared library containing common contracts, messages, and settings.
  - 📌 **Key Components:**
      * `Events/`: Contains all event messages (`OrderStartedEvent`, `PaymentCompletedEvent`, etc.).
      * `Messages/`: Contains all command messages (`StockRollbackMessage`, etc.).
      * `Settings/RabbitMQSettings.cs`: Centralizes all RabbitMQ queue names.

## ⚙️ Setup and Running

1.  Start a RabbitMQ instance.
2.  Start an MSSQL Server and a MongoDB instance.
3.  Update the database and RabbitMQ connection strings in the `appsettings.json` files for each service.
4.  Run database migrations in the `SagaStateMatchineService` project to set up the saga persistence store.
5.  Launch each microservice in the following order: `SagaStateMatchineService`, `OrderAPI`, `StockAPI`, and `PaymentAPI`.
6.  Use the `OrderAPI` endpoints to create an order and test the full saga flow.
