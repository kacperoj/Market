![.NET 8](https://img.shields.io/badge/.NET%208.0-purple?style=flat&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-blue?style=flat)
![OpenAI](https://img.shields.io/badge/AI-GPT--4o%20Vision-green?style=flat&logo=openai)
![Stripe](https://img.shields.io/badge/Payments-Stripe-635bff?style=flat&logo=stripe)
![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?style=flat&logo=postgresql)
![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap%205-7952b3?style=flat&logo=bootstrap)

# 🛒 Market.AI

> **A smart C2C e-commerce platform that solves the problem of tedious listing creation using AI.**
> The system automatically generates specifications, descriptions, and pricing based on product photos. It is fully integrated with a secure wallet (Stripe) and an advanced administration panel.

---

## 🌐 Live Demo
The application is available at: **[market.kacperkotecki.me](https://market.kacperkotecki.me)**

---

## 📺 System Preview

### 1. AI-First Listing ("Snap & Sell")
The user uploads photos, and the **GPT-4o (Vision)** model analyzes the product (recognizing the model, damage, and specs) and fills out the form in a fraction of a second.

<img width="1920" height="1440" alt="418shots_so" src="https://github.com/user-attachments/assets/42f98e0a-758b-4f9a-9f0a-b9f9a3a9b5c0" />

<img width="1920" height="1440" alt="806shots_so" src="https://github.com/user-attachments/assets/3aac6678-debd-408d-9c5f-ef5b127982e6" />

### 2. Wallet and Auctions
A financial dashboard with transaction history and an auction list with filtering.

<img width="1920" height="1440" alt="407shots_so" src="https://github.com/user-attachments/assets/24798596-6d48-45bd-ac01-75c0efe4d008" />

<img width="1920" height="1440" alt="878shots_so" src="https://github.com/user-attachments/assets/e24846a4-5eae-412f-8746-12e6105b368e" />

### 3. Admin Panel (Moderation)
User management and content blocking (Soft Delete).

<img width="1920" height="1440" alt="822shots_so" src="https://github.com/user-attachments/assets/2f5ae7cf-9185-4a3c-97ef-783e1b76db4c" />

### 4. Checkout Process
A seamless checkout flow. The system validates shipping details and redirects users to a secure payment gateway (Stripe Hosted Page), handling real-time transaction status updates.

<img width="1920" height="1440" alt="560shots_so" src="https://github.com/user-attachments/assets/17095d8f-cb88-49cf-82e2-9ad5d3b73ce4" />
<img width="1920" height="1440" alt="715shots_so" src="https://github.com/user-attachments/assets/3e0c27c1-8d96-4da6-aa8f-240d054669d4" />



## 🚀 Key Features & Architecture

### 🧠 1. AI Module (Generative Deep Specs)
The core of the app is the integration with a multimodal LLM (**GPT-4o Vision**) via OpenRouter.
* **Image Analysis:** Photos are sent in Base64 format. AI recognizes the product and generates detailed technical specifications in JSON format.
* **Structured Outputs:** The model's response is automatically deserialized into C# objects, eliminating text parsing errors.

### 💳 2. Hybrid Payment System (Internal Ledger)
Transaction safety is based on an Escrow model.
* **Stripe Webhooks:** Payment status is updated asynchronously (Webhook), preventing client-side manipulation.
* **Internal Wallet:** The system maintains a funds register (`WalletBalance`). Withdrawals are atomic database transactions, ensuring financial consistency (preventing "double spending" errors).

### 🛡️ 3. Security & Clean Code (AOP)
Aspect-Oriented Programming is used for permission management.
* **Custom Attributes:** Instead of cluttering controllers with `if` statements, I used attributes like `[EnsureSellerData]` or `[Buyer]`. They act as filters, checking if a seller has provided company details or an IBAN before allowing them to list an item.
* **Soft Delete:** Blocking users does not physically delete data but changes its state, preserving operation history.

### ⚡ 4. Performance
* **Image Pipeline:** Uploaded photos are automatically processed by **ImageSharp** – scaled to Full HD and converted to **WebP**, reducing file size by ~60-70%.
* **Frontend:** Uses modern **JavaScript (ES6+)** and Fetch API for asynchronous data loading from AI (without page reloads).

---

## 🛠️ Tech Stack

**Backend:**
* .NET 8 (ASP.NET Core MVC)
* Entity Framework Core (Code First)
* PostgreSQL (Database)
* LINQ

**Frontend:**
* Razor Views (`.cshtml`)
* Bootstrap 5 (Dark Mode)
* JavaScript (ES6+) / Fetch API

**Integrations & Tools:**
* **OpenAI (GPT-4o)** – Image analysis and content generation.
* **Stripe** – Online payment processing.
* **ImageSharp** – Image optimization.
* **Docker** – Application containerization.

---

## 📬 Contact

**Kacper Kotecki** 📧 kacperkotecki@protonmail.com
🔗 [LinkedIn Profile](https://www.linkedin.com/in/kacper-kotecki-349829295/)
