# AsiloMicroservicios
# Sistema para Administración del Asilo de Ancianos Cabeza de Algodón

El sistema tiene como objetivo centralizar la información médica, administrativa y financiera del Asilo de Ancianos Cabeza de Algodón.

Permite administrar pacientes internos, familiares responsables, fichas médicas, solicitudes de atención, visitas médicas, laboratorio, farmacia, cobros, pagos, donaciones, gastos, usuarios, roles y notificaciones.

## Tecnologías utilizadas

### Frontend
- Razor Views
- HTML5
- CSS3
- Bootstrap
- JavaScript

### Backend
- C#
- ASP.NET Core MVC
- .NET 10
- ASP.NET Core Web API

### Base de datos
- SQL Server
- Entity Framework Core

### Otras herramientas
- Visual Studio
- SQL Server Management Studio
- Docker
- Docker Compose
- Git
- GitHub

## Arquitectura

El proyecto utiliza una arquitectura web basada en ASP.NET Core MVC y separación por capas.

Principales componentes:

- Capa de presentación
- Controladores
- Lógica de negocio
- Acceso a datos
- Base de datos SQL Server
- Microservicio de notificaciones

## Módulos del sistema

- Autenticación y autorización
- Gestión de usuarios y roles
- Gestión de internos
- Gestión de familiares
- Ficha médica
- Solicitudes médicas
- Visitas médicas
- Laboratorio
- Farmacia
- Finanzas
- Reportes
- Notificaciones

## Seguridad

El sistema contempla:

- Autenticación mediante cookies
- Autorización basada en roles
- Claims
- Restricción de acceso mediante Authorize
- Control de permisos
- Pantalla de acceso denegado
- Protección de información médica y administrativa

## Estructura principal

Asilo-Cabeza-de-Algodon
│
├── Asilo.Web
│
├── Asilo.Notificaciones.Api
├── docker-compose.yml
├── .gitignore
└── README.md
