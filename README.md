# YemekSepetiWeb - Food Ordering System

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-purple?style=flat&logo=dotnet)
![ASP.NET MVC](https://img.shields.io/badge/Architecture-MVC-blue)
![Entity Framework](https://img.shields.io/badge/ORM-Entity%20Framework%20Core-cyan)
![SQL Server](https://img.shields.io/badge/Database-MS%20SQL%20Server-red)

## About the Project
This project was developed during my 3rd year of Computer Engineering studies to solidify my understanding of the **ASP.NET Core MVC** architecture, relational database design, and state management. 

Rather than relying on ready-made identity templates, I built this monolithic web application to get hands-on experience with core backend concepts, including CRUD operations, Entity Framework Core (Database/Code-First), and Session-based authentication. It serves as a fundamental stepping stone before moving on to microservices and more complex API architectures.

## Key Features
* **Role-Based Routing:** Separate views and logic for Admins and Customers.
* **Customer Panel:** Browse restaurants, view menus, and place orders dynamically.
* **Admin Dashboard:** Full CRUD management over restaurants, menu items, and user records.
* **Session Management:** Custom implementation of login/logout mechanisms using `HttpContext.Session`.

## Technologies Used
* **Backend:** C#, ASP.NET Core MVC 8.0
* **ORM:** Entity Framework Core
* **Database:** Microsoft SQL Server
* **Frontend:** HTML5, CSS3, Bootstrap 4/5, Razor Syntax

## How to Run the Project (Deployment)
I have included a database script so you can easily replicate the environment on your local machine.

1. Clone the repository: `git clone https://github.com/YOUR_GITHUB_USERNAME/YemekSepetiWeb.git`
2. Open **Microsoft SQL Server Management Studio (SSMS)**.
3. Open the `YemekSepeti_Db_Yedek.sql` file included in the project directory and execute it. This will automatically create the schema and populate the tables with initial test data.
4. Update the `appsettings.json` file with your local SQL Server connection string.
5. Open the `.sln` file in **Visual Studio** and press `F5` to run the application.

## Future Improvements (To-Do)
As I continue to grow as a developer, I plan to refactor this project with the following industry standards:
- [ ] Migrate from basic Session authentication to **ASP.NET Core Identity**.
- [ ] Implement secure password hashing (BCrypt/Argon2) instead of plaintext storage.
- [ ] Decouple the monolithic structure into an API-first approach (Web API + React/Angular frontend).
- [ ] Add Unit Tests (xUnit/Moq) for the controller logic.

---
*Developed by a passionate Computer Engineering student looking for internship opportunities to apply academic knowledge to real-world production environments.*
