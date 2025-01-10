# eLibrary Project
======================

## Overview

eLibrary is a web-based library management system built using ASP.NET Core and Entity Framework Core. The project allows users to browse and borrow books, manage their account, and receive notifications when books become available.

## Features

* User registration and login
* Book browsing and borrowing
* User account management
* Waiting list management
* Email notifications for book availability, Checkout summary
* Payment processing using PayPal

## Technologies Used

* ASP.NET Core
* Entity Framework Core
* SQLite
* PayPal API
* JavaScript and HTML/CSS for front-end development

## Project Structure

The project is organized into the following folders:

* `Controllers`: Contains the ASP.NET Core controllers for handling user requests.
* `Models`: Defines the data models for the application, including books, users, and waiting lists.
* `Services`: Contains the services for sending emails and processing payments.
* `Views`: Holds the Razor views for the application's user interface.
* `wwwroot`: Contains the static files for the application, including images, CSS, and JavaScript files.

## Database

The project uses a SQLite database to store data. The database schema is defined in the `DB_context.cs` file.

## Configuration

The project uses the `appsettings.json` file for configuration. This file contains settings for the database connection, PayPal API, and email service.

## Running the Project

To run the project, follow these steps:

1. Clone the repository to your local machine.
2. Open the project in Visual Studio.
3. Restore the NuGet packages.
4. Update the `appsettings.json` file with your own database connection and PayPal API settings.
5. Run the project using the `dotnet run` command.
