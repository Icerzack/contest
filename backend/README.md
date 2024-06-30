# CamelotCS (Backend)

This is the backend repository of the CamelotCS project.

## How to run the project

> [!IMPORTANT]
> This project requires Docker to be installed on your machine. If you don't have Docker installed, please visit [docker.com](https://www.docker.com/) and follow the instructions.

1. Clone the repository
2. Navigate to the root directory of the project and run the following command:

   ```bash
   make run-all
   ```

   This will build and run the ASP.NET Backend and PostgreSQL database.

   If you want to run something specific, you can run the following commands:

   ```bash
   make run-db
   ```

   or

   ```bash
   make run-app
   ```

3. The project should be running on `http://localhost:5001/`.

## How to register a user

To register a user, you need to send a POST request to `api/v1/users/register` with the following body:

```json
{
  "username": "username",
  "password": "password"
}
```

There is also an optional Header `X-Register-Key` which is used to disable the "simple" registration.

For example, if the `X-Register-Key` is set to `1234`, the server will only accept the request if it contains the following header:

```
"X-Register-Key": "1234"
```

The `X-Register-Key` is set via the environment variable `REGISTER_KEY`.

By default, the `X-Register-Key` is not set, so the registration is open to everyone.

## Features

- **Authentication & Authorization**: The project has an authentication and authorization system that allows users to register and login.
- **Collections management**: Users can create, update, delete and view _personal_ collections.
- **Boards management**: Users can create, update, delete and view _personal_ boards. Boards are grouped into collections.

## Technologies used

- **ASP.NET Core 7.0**: The project is built using ASP.NET Core 7.0.
- **PostgreSQL**: The project uses PostgreSQL 15 as the database.
- **Docker**: The project uses Docker to containerize the application and the database.
- **Entity Framework Core**: The project uses Entity Framework Core to interact with the database.
- **JWT**: The project uses JWT for authentication.
- **Swagger**: The project uses Swagger for API documentation.
