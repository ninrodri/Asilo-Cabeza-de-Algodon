using Asilo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Data
{
    public static class DbSeeder
    {
        private static readonly string[] Especialidades =
        {
            "Cardiología",
            "Neurología",
            "Psiquiatría",
            "Traumatología",
            "Medicina interna",
            "Geriatría",
            "Dermatología",
            "Oftalmología"
        };

        public static async Task RecrearDatosDemoAsync(
            AsiloDbContext context)
        {
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            try
            {
                await LimpiarBaseDatosAsync(context);

                var roles = await CrearRolesAsync(context);

                var usuarios = await CrearUsuariosAsync(
                    context,
                    roles
                );

                var pacientes = await CrearPacientesAsync(context);

                await CrearFamiliaresAsync(
                    context,
                    pacientes
                );

                await CrearFichasMedicasAsync(
                    context,
                    pacientes
                );

                var consultas = await CrearConsultasGeneralesAsync(
                    context,
                    pacientes,
                    usuarios.MedicosGenerales
                );

                var solicitudes = await CrearSolicitudesAsync(
                    context,
                    consultas,
                    usuarios.Especialistas,
                    usuarios.Enfermeros
                );

                var visitas = await CrearVisitasAsync(
                    context,
                    solicitudes
                );

                var examenes = await CrearExamenesAsync(
                    context,
                    visitas
                );

                var recetas = await CrearRecetasAsync(
                    context,
                    visitas
                );

                await CrearHistorialesAsync(
                    context,
                    consultas,
                    visitas
                );

                await CrearCobrosMedicosAsync(
                    context,
                    visitas,
                    examenes,
                    recetas
                );

                await CrearCuotasMensualesAsync(
                    context,
                    pacientes
                );

                await CrearDonacionesAsync(context);
                await CrearGastosAsync(context);
                await CrearPagosFundacionAsync(context);

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // LIMPIEZA
        // =========================================================

        private static async Task LimpiarBaseDatosAsync(
            AsiloDbContext context)
        {
            // Primero eliminamos las tablas dependientes.
            await context.PagosFundacion.ExecuteDeleteAsync();

            await context.Pagos.ExecuteDeleteAsync();

            await context.CuotasMensuales.ExecuteDeleteAsync();

            await context.CobrosVisitas.ExecuteDeleteAsync();

            await context.DetallesCobro.ExecuteDeleteAsync();

            await context.Cobros.ExecuteDeleteAsync();

            await context.DetallesReceta.ExecuteDeleteAsync();

            await context.Recetas.ExecuteDeleteAsync();

            await context.ExamenesLaboratorio.ExecuteDeleteAsync();

            await context.VisitasMedicas.ExecuteDeleteAsync();

            await context.SolicitudesMedicas.ExecuteDeleteAsync();

            await context.ConsultasGenerales.ExecuteDeleteAsync();

            await context.HistorialesMedicos.ExecuteDeleteAsync();

            await context.FichasMedicas.ExecuteDeleteAsync();

            await context.Familiares.ExecuteDeleteAsync();

            await context.Donaciones.ExecuteDeleteAsync();

            await context.Gastos.ExecuteDeleteAsync();

            await context.Pacientes.ExecuteDeleteAsync();

            await context.Usuarios.ExecuteDeleteAsync();

            await context.Roles.ExecuteDeleteAsync();
        }

        // =========================================================
        // ROLES
        // =========================================================

        private static async Task<Dictionary<string, Rol>>
            CrearRolesAsync(AsiloDbContext context)
        {
            var nombres = new[]
            {
                "Administrador",
                "Personal administrativo",
                "Médico general",
                "Médico especialista",
                "Fundación",
                "Laboratorio",
                "Farmacia",
                "Enfermero"
            };

            var roles = nombres
                .Select(nombre => new Rol
                {
                    Nombre = nombre
                })
                .ToList();

            context.Roles.AddRange(roles);

            await context.SaveChangesAsync();

            return roles.ToDictionary(
                r => r.Nombre,
                r => r
            );
        }

        // =========================================================
        // USUARIOS
        // =========================================================

        private class UsuariosSeed
        {
            public List<Usuario> MedicosGenerales { get; set; }
                = new();

            public List<Usuario> Especialistas { get; set; }
                = new();

            public List<Usuario> Enfermeros { get; set; }
                = new();
        }

        private static async Task<UsuariosSeed>
            CrearUsuariosAsync(
                AsiloDbContext context,
                Dictionary<string, Rol> roles)
        {
            var usuarios = new List<Usuario>();

            void AgregarUsuario(
                string nombre,
                string correo,
                string rol,
                string? especialidad = null)
            {
                usuarios.Add(new Usuario
                {
                    NombreUsuario = nombre,
                    Correo = correo,
                    PasswordHash = "1234",
                    Estado = true,
                    RolId = roles[rol].Id,
                    Especialidad = especialidad
                });
            }

            // ADMINISTRADOR
            AgregarUsuario(
                "abigail.chavez",
                "abigail.chavez@asilo.test",
                "Administrador"
            );

            // PERSONAL ADMINISTRATIVO
            AgregarUsuario(
                "laura.mendez",
                "laura.mendez@asilo.test",
                "Personal administrativo"
            );

            AgregarUsuario(
                "carlos.ramirez",
                "carlos.ramirez@asilo.test",
                "Personal administrativo"
            );

            // MÉDICOS GENERALES
            var generales = new[]
            {
                ("ana.lopez", "ana.lopez@asilo.test"),
                ("jorge.perez", "jorge.perez@asilo.test"),
                ("sofia.morales", "sofia.morales@asilo.test"),
                ("carlos.mendez", "carlos.mendez@asilo.test"),
                ("lucia.garcia", "lucia.garcia@asilo.test")
            };

            foreach (var medico in generales)
            {
                AgregarUsuario(
                    medico.Item1,
                    medico.Item2,
                    "Médico general"
                );
            }

            // 40 MÉDICOS ESPECIALISTAS
            var especialistas = new Dictionary<string, string[]>
            {
                ["Cardiología"] = new[]
                {
                    "daniel.reyes",
                    "maria.castillo",
                    "fernando.soto",
                    "elena.rivas",
                    "miguel.ortiz"
                },

                ["Neurología"] = new[]
                {
                    "paola.herrera",
                    "andres.molina",
                    "valeria.castro",
                    "roberto.guzman",
                    "diana.fuentes"
                },

                ["Psiquiatría"] = new[]
                {
                    "gabriela.arias",
                    "manuel.cabrera",
                    "claudia.estrada",
                    "oscar.leon",
                    "patricia.mejia"
                },

                ["Traumatología"] = new[]
                {
                    "ricardo.monterroso",
                    "alejandra.ruiz",
                    "esteban.villatoro",
                    "monica.salazar",
                    "julio.alarcon"
                },

                ["Medicina interna"] = new[]
                {
                    "beatriz.moreno",
                    "hector.rosales",
                    "adriana.lemus",
                    "sergio.galindo",
                    "veronica.delgado"
                },

                ["Geriatría"] = new[]
                {
                    "marcela.pineda",
                    "eduardo.sandoval",
                    "karla.aguilar",
                    "rene.cifuentes",
                    "silvia.bautista"
                },

                ["Dermatología"] = new[]
                {
                    "natalia.mendoza",
                    "armando.rojas",
                    "carolina.velasquez",
                    "javier.escobar",
                    "irene.maldonado"
                },

                ["Oftalmología"] = new[]
                {
                    "mariana.solis",
                    "pedro.barrera",
                    "angela.navarro",
                    "tomas.mendez",
                    "isabel.ordonez"
                }
            };

            foreach (var grupo in especialistas)
            {
                foreach (var nombre in grupo.Value)
                {
                    AgregarUsuario(
                        nombre,
                        $"{nombre}@asilo.test",
                        "Médico especialista",
                        grupo.Key
                    );
                }
            }

            // ENFERMEROS
            var enfermeros = new[]
            {
                "marta.hernandez",
                "julio.santos",
                "rosa.morales",
                "alan.gomez",
                "luisa.mendez"
            };

            foreach (var nombre in enfermeros)
            {
                AgregarUsuario(
                    nombre,
                    $"{nombre}@asilo.test",
                    "Enfermero"
                );
            }

            // FUNDACIÓN
            AgregarUsuario(
                "mariana.fundacion",
                "mariana.fundacion@asilo.test",
                "Fundación"
            );

            AgregarUsuario(
                "diego.fundacion",
                "diego.fundacion@asilo.test",
                "Fundación"
            );

            // LABORATORIO
            AgregarUsuario(
                "andrea.laboratorio",
                "andrea.laboratorio@asilo.test",
                "Laboratorio"
            );

            AgregarUsuario(
                "jose.laboratorio",
                "jose.laboratorio@asilo.test",
                "Laboratorio"
            );

            // FARMACIA
            AgregarUsuario(
                "silvia.farmacia",
                "silvia.farmacia@asilo.test",
                "Farmacia"
            );

            AgregarUsuario(
                "ricardo.farmacia",
                "ricardo.farmacia@asilo.test",
                "Farmacia"
            );

            context.Usuarios.AddRange(usuarios);

            await context.SaveChangesAsync();

            return new UsuariosSeed
            {
                MedicosGenerales = usuarios
                    .Where(u =>
                        u.RolId == roles["Médico general"].Id)
                    .ToList(),

                Especialistas = usuarios
                    .Where(u =>
                        u.RolId == roles["Médico especialista"].Id)
                    .ToList(),

                Enfermeros = usuarios
                    .Where(u =>
                        u.RolId == roles["Enfermero"].Id)
                    .ToList()
            };
        }

        // =========================================================
        // PACIENTES
        // =========================================================

        private static async Task<List<Anciano>>
            CrearPacientesAsync(AsiloDbContext context)
        {
            var nombres = new[]
            {
                "María Teresa López García",
                "José Antonio Morales Pérez",
                "Rosa Amelia Hernández Soto",
                "Francisco Javier Méndez García",
                "Teresa del Carmen López Ruiz",
                "Carlos Enrique Ramírez Díaz",
                "Ana Lucía Castillo Gómez",
                "Manuel de Jesús Herrera Pérez",
                "Carmen Elena Vásquez López",
                "Pedro Antonio García Méndez",
                "Marta Julia Escobar Reyes",
                "Luis Alberto Morales Soto",
                "Elena Beatriz Cabrera Ruiz",
                "Jorge Mario Fuentes López",
                "Isabel Cristina Méndez Pérez",
                "Roberto Carlos Salazar Díaz",
                "Dolores Esperanza Reyes Gómez",
                "Miguel Ángel Castro López",
                "Olga Marina Pérez Morales",
                "Ernesto Antonio Gómez Ruiz"
            };

            var familiares = new[]
            {
                "Andrea López Méndez",
                "Carlos Morales Ramírez",
                "Lucía Hernández Díaz",
                "Mario Méndez Soto",
                "Patricia López Ruiz",
                "Gabriela Ramírez Díaz",
                "Julio Castillo Gómez",
                "Ana Herrera Pérez",
                "Fernando Vásquez López",
                "María García Méndez",
                "Sofía Escobar Reyes",
                "Raúl Morales Soto",
                "Claudia Cabrera Ruiz",
                "Rosa Fuentes López",
                "Daniel Méndez Pérez",
                "Laura Salazar Díaz",
                "José Reyes Gómez",
                "Mónica Castro López",
                "Pedro Pérez Morales",
                "Elena Gómez Ruiz"
            };

            var pacientes = new List<Anciano>();

            for (int i = 0; i < 20; i++)
            {
                pacientes.Add(new Anciano
                {
                    NombreCompleto = nombres[i],

                    FechaNacimiento =
                        new DateTime(
                            1935 + (i % 18),
                            (i % 12) + 1,
                            (i % 20) + 1
                        ),

                    Sexo = i % 2 == 0
                        ? "Femenino"
                        : "Masculino",

                    FechaIngreso =
                        DateTime.Today
                            .AddMonths(-(i + 3)),

                    MotivoIngreso =
                        i % 3 == 0
                            ? "Requiere cuidados permanentes y supervisión médica."
                            : i % 3 == 1
                                ? "Necesita apoyo para actividades de la vida diaria."
                                : "Ingreso por necesidad de acompañamiento y atención integral.",

                    FamiliarResponsable =
                        familiares[i],

                    TelefonoFamiliar =
                        $"55{20 + i:D2}-{10 + i:D2}{30 + i:D2}",

                    CorreoFamiliar =
                        $"familiar{i + 1}@correo.test",

                    Padecimientos =
                        (i % 4) switch
                        {
                            0 => "Hipertensión arterial",
                            1 => "Diabetes tipo 2",
                            2 => "Artritis",
                            _ => "Sin padecimientos graves registrados"
                        },

                    MedicamentosActuales =
                        (i % 4) switch
                        {
                            0 => "Losartán 50 mg",
                            1 => "Metformina 850 mg",
                            2 => "Paracetamol según dolor",
                            _ => "Ninguno"
                        }
                });
            }

            context.Pacientes.AddRange(pacientes);

            await context.SaveChangesAsync();

            return pacientes;
        }

        // =========================================================
        // FAMILIARES
        // =========================================================

        private static async Task CrearFamiliaresAsync(
            AsiloDbContext context,
            List<Anciano> pacientes)
        {
            var parentescos = new[]
            {
                "Hija",
                "Hijo",
                "Hermana",
                "Hermano",
                "Sobrina"
            };

            var familiares = pacientes
                .Select((p, i) => new Familiar
                {
                    NombreCompleto =
                        p.FamiliarResponsable,

                    Parentesco =
                        parentescos[i % parentescos.Length],

                    Telefono =
                        p.TelefonoFamiliar,

                    Correo =
                        p.CorreoFamiliar,

                    Direccion =
                        i % 2 == 0
                            ? "Mazatenango, Suchitepéquez"
                            : "Retalhuleu, Guatemala",

                    PacienteId = p.Id
                })
                .ToList();

            context.Familiares.AddRange(familiares);

            await context.SaveChangesAsync();
        }

        // =========================================================
        // FICHAS MÉDICAS
        // =========================================================

        private static async Task CrearFichasMedicasAsync(
            AsiloDbContext context,
            List<Anciano> pacientes)
        {
            var fichas = pacientes
                .Select((p, i) => new FichaMedica
                {
                    PacienteId = p.Id,

                    Padecimientos = p.Padecimientos,

                    Enfermedades =
                        (i % 3) switch
                        {
                            0 => "Hipertensión arterial",
                            1 => "Diabetes mellitus tipo 2",
                            _ => "Artrosis"
                        },

                    Psicopatologias =
                        i % 5 == 0
                            ? "Ansiedad leve"
                            : "Ninguna registrada",

                    MedicamentosFrecuentes =
                        p.MedicamentosActuales,

                    Alergias =
                        i % 4 == 0
                            ? "Penicilina"
                            : "Ninguna conocida",

                    ObservacionesMedicas =
                        "Paciente requiere seguimiento periódico.",

                    FechaActualizacion =
                        DateTime.Today.AddDays(-i)
                })
                .ToList();

            context.FichasMedicas.AddRange(fichas);

            await context.SaveChangesAsync();
        }

        // =========================================================
        // CONSULTAS GENERALES
        // =========================================================

        private static async Task<List<ConsultaGeneral>>
            CrearConsultasGeneralesAsync(
                AsiloDbContext context,
                List<Anciano> pacientes,
                List<Usuario> medicos)
        {
            var consultas = new List<ConsultaGeneral>();

            for (int i = 0; i < 30; i++)
            {
                bool requiereEspecialista = i < 28;

                var especialidad =
                    requiereEspecialista
                        ? Especialidades[i % Especialidades.Length]
                        : null;

                var consulta = new ConsultaGeneral
                {
                    PacienteId =
                        pacientes[i % pacientes.Count].Id,

                    FechaConsulta =
                        DateTime.Today
                            .AddDays(-(35 - i))
                            .AddHours(9),

                    Medico =
                        medicos[i % medicos.Count]
                            .NombreUsuario,

                    Motivo =
                        (i % 4) switch
                        {
                            0 => "Mareos y presión arterial elevada.",
                            1 => "Dolor persistente y dificultad para movilizarse.",
                            2 => "Alteraciones del sueño y cambios de ánimo.",
                            _ => "Control médico general y seguimiento."
                        },

                    Diagnostico =
                        (i % 4) switch
                        {
                            0 => "Hipertensión arterial en evaluación.",
                            1 => "Dolor osteomuscular pendiente de evaluación especializada.",
                            2 => "Trastorno del sueño en estudio.",
                            _ => "Estado general estable."
                        },

                    Observaciones =
                        "Se recomienda seguimiento clínico.",

                    RequiereEspecialista =
                        requiereEspecialista,

                    EspecialidadRequerida =
                        especialidad,

                    MotivoReferencia =
                        requiereEspecialista
                            ? $"Se solicita evaluación por {especialidad}."
                            : null
                };

                consultas.Add(consulta);
            }

            context.ConsultasGenerales.AddRange(consultas);

            await context.SaveChangesAsync();

            return consultas;
        }

        // =========================================================
        // SOLICITUDES MÉDICAS
        // =========================================================

        private static async Task<List<SolicitudMedica>>
            CrearSolicitudesAsync(
                AsiloDbContext context,
                List<ConsultaGeneral> consultas,
                List<Usuario> especialistas,
                List<Usuario> enfermeros)
        {
            var referencias = consultas
                .Where(c => c.RequiereEspecialista)
                .Take(28)
                .ToList();

            var solicitudes =
                new List<SolicitudMedica>();

            for (int i = 0; i < referencias.Count; i++)
            {
                var consulta = referencias[i];

                string estado;

                if (i < 4)
                    estado = "Pendiente de Fundación";
                else if (i < 8)
                    estado = "Programada";
                else
                    estado = "Atendida";

                var solicitud = new SolicitudMedica
                {
                    PacienteId =
                        consulta.PacienteId,

                    FechaSolicitud =
                        consulta.FechaConsulta
                            .AddHours(1),

                    Motivo =
                        consulta.MotivoReferencia
                        ?? "Evaluación especializada.",

                    Observaciones =
                        consulta.Observaciones,

                    Estado =
                        estado,

                    Especialidad =
                        consulta.EspecialidadRequerida
                };

                if (estado != "Pendiente de Fundación")
                {
                    var especialistasDisponibles =
                        especialistas
                            .Where(e =>
                                e.Especialidad ==
                                consulta.EspecialidadRequerida)
                            .ToList();

                    var especialista =
                        especialistasDisponibles[
                            i % especialistasDisponibles.Count
                        ];

                    var enfermero =
                        enfermeros[i % enfermeros.Count];

                    solicitud.MedicoEspecialistaId =
                        especialista.Id;

                    solicitud.EnfermeroId =
                        enfermero.Id;

                    solicitud.FechaAtencion =
                        consulta.FechaConsulta
                            .AddDays(3)
                            .AddHours(2);
                }

                solicitudes.Add(solicitud);
            }

            context.SolicitudesMedicas.AddRange(solicitudes);

            await context.SaveChangesAsync();

            return solicitudes;
        }

        // =========================================================
        // VISITAS ESPECIALIZADAS
        // =========================================================

        private static async Task<List<VisitaMedica>>
            CrearVisitasAsync(
                AsiloDbContext context,
                List<SolicitudMedica> solicitudes)
        {
            // Las primeras 4 están pendientes.
            // Las siguientes 4 están programadas.
            // Las últimas 20 están atendidas.
            var atendidas = solicitudes
                .Where(s => s.Estado == "Atendida")
                .Take(20)
                .ToList();

            var visitas =
                new List<VisitaMedica>();

            foreach (var solicitud in atendidas)
            {
                var visita = new VisitaMedica
                {
                    SolicitudMedicaId =
                        solicitud.Id,

                    PacienteId =
                        solicitud.PacienteId,

                    FechaVisita =
                        solicitud.FechaAtencion
                        ?? DateTime.Today,

                    Medico =
                        solicitud.MedicoEspecialista?
                            .NombreUsuario
                        ?? "Médico especialista",

                    Diagnostico =
                        $"Evaluación realizada por {solicitud.Especialidad}.",

                    Tratamiento = "",

                    Indicaciones = "",

                    Observaciones =
                        "Paciente evaluado. Continuar seguimiento clínico.",

                    CostoConsulta =
                        150m +
                        ((visitas.Count % 5) * 25m)
                };

                visitas.Add(visita);
            }

            // Si las navegaciones no están cargadas,
            // recuperamos los nombres reales.
            foreach (var visita in visitas)
            {
                var solicitud = solicitudes
                    .First(s =>
                        s.Id == visita.SolicitudMedicaId);

                if (solicitud.MedicoEspecialistaId.HasValue)
                {
                    visita.Medico =
                        await context.Usuarios
                            .Where(u =>
                                u.Id ==
                                solicitud.MedicoEspecialistaId.Value)
                            .Select(u => u.NombreUsuario)
                            .FirstAsync();
                }
            }

            context.VisitasMedicas.AddRange(visitas);

            await context.SaveChangesAsync();

            return visitas;
        }

        // =========================================================
        // LABORATORIO
        // =========================================================

        private static async Task<List<ExamenLaboratorio>>
            CrearExamenesAsync(
                AsiloDbContext context,
                List<VisitaMedica> visitas)
        {
            var nombres = new[]
            {
                "Hemograma completo",
                "Glucosa en sangre",
                "Perfil lipídico",
                "Examen general de orina",
                "Creatinina",
                "Ácido úrico",
                "Pruebas hepáticas",
                "Electrolitos séricos"
            };

            var examenes =
                new List<ExamenLaboratorio>();

            for (int i = 0; i < visitas.Count; i++)
            {
                bool completado = i < 16;

                examenes.Add(new ExamenLaboratorio
                {
                    PacienteId =
                        visitas[i].PacienteId,

                    VisitaMedicaId =
                        visitas[i].Id,

                    NombreExamen =
                        nombres[i % nombres.Length],

                    FechaSolicitud =
                        visitas[i].FechaVisita
                            .AddHours(1),

                    Estado =
                        completado
                            ? "Completado"
                            : "Solicitado",

                    Resultado =
                        completado
                            ? "Resultado revisado. Valores compatibles con seguimiento clínico."
                            : "",

                    FechaResultado =
                        completado
                            ? visitas[i].FechaVisita
                                .AddDays(1)
                            : null,

                    Observaciones =
                        "Realizar según indicaciones del médico.",

                    Costo =
                        completado
                            ? 75m + ((i % 6) * 25m)
                            : 0m
                });
            }

            context.ExamenesLaboratorio
                .AddRange(examenes);

            await context.SaveChangesAsync();

            return examenes;
        }

        // =========================================================
        // RECETAS Y MEDICAMENTOS
        // =========================================================

        private static async Task<List<Receta>>
            CrearRecetasAsync(
                AsiloDbContext context,
                List<VisitaMedica> visitas)
        {
            var medicamentos = new[]
            {
                ("Losartán 50 mg", "1 tableta", "Cada 24 horas", 30, 2.00m),
                ("Metformina 850 mg", "1 tableta", "Cada 12 horas", 30, 1.75m),
                ("Paracetamol 500 mg", "1 tableta", "Cada 8 horas", 20, 1.50m),
                ("Omeprazol 20 mg", "1 cápsula", "Cada 24 horas", 30, 1.25m),
                ("Aspirina 100 mg", "1 tableta", "Cada 24 horas", 30, 1.10m),
                ("Calcio 600 mg", "1 tableta", "Cada 24 horas", 30, 2.50m),
                ("Diclofenaco 50 mg", "1 tableta", "Cada 12 horas", 15, 1.80m),
                ("Atorvastatina 20 mg", "1 tableta", "Por la noche", 30, 3.00m)
            };

            var recetas = new List<Receta>();

            for (int i = 0; i < visitas.Count; i++)
            {
                string estado;

                if (i < 16)
                    estado = "Entregada";
                else if (i < 18)
                    estado = "Emitida";
                else
                    estado = "Pendiente";

                var receta = new Receta
                {
                    PacienteId =
                        visitas[i].PacienteId,

                    VisitaMedicaId =
                        visitas[i].Id,

                    FechaReceta =
                        visitas[i].FechaVisita,

                    Medico =
                        visitas[i].Medico,

                    IndicacionesGenerales = "",

                    Estado = estado
                };

                var med1 =
                    medicamentos[i % medicamentos.Length];

                var med2 =
                    medicamentos[
                        (i + 3) % medicamentos.Length
                    ];

                receta.Detalles.Add(
                    new DetalleReceta
                    {
                        Medicamento = med1.Item1,
                        Dosis = med1.Item2,
                        Frecuencia = med1.Item3,
                        Duracion = "30 días",
                        Indicaciones =
                            "Tomar con agua según indicación médica.",

                        Cantidad =
                            estado == "Pendiente"
                                ? 1
                                : med1.Item4,

                        PrecioUnitario =
                            estado == "Pendiente"
                                ? 0
                                : med1.Item5
                    }
                );

                receta.Detalles.Add(
                    new DetalleReceta
                    {
                        Medicamento = med2.Item1,
                        Dosis = med2.Item2,
                        Frecuencia = med2.Item3,
                        Duracion = "30 días",
                        Indicaciones =
                            "Mantener horario regular.",

                        Cantidad =
                            estado == "Pendiente"
                                ? 1
                                : med2.Item4,

                        PrecioUnitario =
                            estado == "Pendiente"
                                ? 0
                                : med2.Item5
                    }
                );

                recetas.Add(receta);
            }

            context.Recetas.AddRange(recetas);

            await context.SaveChangesAsync();

            return recetas;
        }

        // =========================================================
        // HISTORIAL MÉDICO
        // =========================================================

        private static async Task CrearHistorialesAsync(
            AsiloDbContext context,
            List<ConsultaGeneral> consultas,
            List<VisitaMedica> visitas)
        {
            var historial =
                new List<HistorialMedico>();

            foreach (var consulta in consultas)
            {
                historial.Add(new HistorialMedico
                {
                    PacienteId =
                        consulta.PacienteId,

                    Fecha =
                        consulta.FechaConsulta,

                    TipoRegistro =
                        "Consulta médica general",

                    Descripcion =
                        $"{consulta.Motivo} Diagnóstico: {consulta.Diagnostico}",

                    Observaciones =
                        consulta.Observaciones,

                    ProfesionalResponsable =
                        consulta.Medico
                });
            }

            foreach (var visita in visitas)
            {
                historial.Add(new HistorialMedico
                {
                    PacienteId =
                        visita.PacienteId,

                    Fecha =
                        visita.FechaVisita,

                    TipoRegistro =
                        "Visita médica especializada",

                    Descripcion =
                        visita.Diagnostico,

                    Observaciones =
                        visita.Observaciones,

                    ProfesionalResponsable =
                        visita.Medico
                });
            }

            context.HistorialesMedicos
                .AddRange(historial);

            await context.SaveChangesAsync();
        }

        // =========================================================
        // COBROS MÉDICOS
        // =========================================================

        private static async Task CrearCobrosMedicosAsync(
            AsiloDbContext context,
            List<VisitaMedica> visitas,
            List<ExamenLaboratorio> examenes,
            List<Receta> recetas)
        {
            // Solo las primeras 16 están totalmente listas:
            // laboratorio completado + farmacia entregada.
            var visitasListas =
                visitas.Take(16).ToList();

            for (int i = 0; i < visitasListas.Count; i++)
            {
                var visita =
                    visitasListas[i];

                var examen =
                    examenes.First(e =>
                        e.VisitaMedicaId == visita.Id);

                var receta =
                    recetas.First(r =>
                        r.VisitaMedicaId == visita.Id);

                decimal medicamentos =
                    receta.Detalles.Sum(
                        d => d.Cantidad *
                             d.PrecioUnitario
                    );

                decimal subtotal =
                    visita.CostoConsulta +
                    examen.Costo +
                    medicamentos;

                decimal porcentajeDescuento = 20m;

                decimal montoDescuento =
                    subtotal *
                    porcentajeDescuento /
                    100m;

                decimal total =
                    subtotal - montoDescuento;

                string estado;

                if (i < 5)
                    estado = "Pendiente";
                else if (i < 10)
                    estado = "Parcial";
                else
                    estado = "Pagado";

                var cobro = new Cobro
                {
                    PacienteId =
                        visita.PacienteId,

                    VisitaMedicaId = null,

                    FechaCobro =
                        visita.FechaVisita
                            .AddDays(2),

                    Subtotal =
                        subtotal,

                    PorcentajeDescuento =
                        porcentajeDescuento,

                    MontoDescuento =
                        montoDescuento,

                    Total =
                        total,

                    Estado =
                        estado
                };

                cobro.Detalles.Add(
                    new DetalleCobro
                    {
                        Tipo = "Consulta médica",

                        Descripcion =
                            $"Consulta médica #{visita.Id} - " +
                            $"{visita.Medico} - " +
                            $"{visita.FechaVisita:dd/MM/yyyy}",

                        Monto =
                            visita.CostoConsulta
                    }
                );

                cobro.Detalles.Add(
                    new DetalleCobro
                    {
                        Tipo = "Laboratorio",

                        Descripcion =
                            examen.NombreExamen,

                        Monto =
                            examen.Costo
                    }
                );

                cobro.Detalles.Add(
                    new DetalleCobro
                    {
                        Tipo = "Medicamento",

                        Descripcion =
                            $"Medicamentos de receta #{receta.Id}",

                        Monto =
                            medicamentos
                    }
                );

                context.Cobros.Add(cobro);

                await context.SaveChangesAsync();

                context.CobrosVisitas.Add(
                    new CobroVisita
                    {
                        CobroId =
                            cobro.Id,

                        VisitaMedicaId =
                            visita.Id
                    }
                );

                if (estado == "Parcial")
                {
                    context.Pagos.Add(
                        new Pago
                        {
                            CobroId = cobro.Id,

                            FechaPago =
                                cobro.FechaCobro
                                    .AddDays(1),

                            Monto =
                                Math.Round(
                                    total / 2m,
                                    2
                                ),

                            MetodoPago =
                                "Efectivo",

                            Referencia =
                                $"PAR-{cobro.Id:D4}",

                            Observaciones =
                                "Pago parcial del familiar."
                        }
                    );
                }

                if (estado == "Pagado")
                {
                    context.Pagos.Add(
                        new Pago
                        {
                            CobroId = cobro.Id,

                            FechaPago =
                                cobro.FechaCobro
                                    .AddDays(1),

                            Monto =
                                total,

                            MetodoPago =
                                i % 2 == 0
                                    ? "Efectivo"
                                    : "Transferencia",

                            Referencia =
                                $"PAG-{cobro.Id:D4}",

                            Observaciones =
                                "Cobro cancelado completamente."
                        }
                    );
                }

                await context.SaveChangesAsync();
            }
        }

        // =========================================================
        // CUOTAS MENSUALES
        // =========================================================

        private static async Task CrearCuotasMensualesAsync(
            AsiloDbContext context,
            List<Anciano> pacientes)
        {
            for (int i = 0; i < pacientes.Count; i++)
            {
                decimal monto =
                    600m + ((i % 5) * 100m);

                string estado;

                if (i < 7)
                    estado = "Pendiente";
                else if (i < 13)
                    estado = "Parcial";
                else
                    estado = "Pagado";

                var cobro = new Cobro
                {
                    PacienteId =
                        pacientes[i].Id,

                    FechaCobro =
                        DateTime.Today
                            .AddDays(-(20 - i)),

                    Subtotal = monto,

                    PorcentajeDescuento = 0,

                    MontoDescuento = 0,

                    Total = monto,

                    Estado = estado
                };

                cobro.Detalles.Add(
                    new DetalleCobro
                    {
                        Tipo =
                            "Cuota mensual",

                        Descripcion =
                            $"Cuota mensual correspondiente a " +
                            $"{DateTime.Today:MMMM yyyy}",

                        Monto =
                            monto
                    }
                );

                context.Cobros.Add(cobro);

                await context.SaveChangesAsync();

                var cuota = new CuotaMensual
                {
                    PacienteId =
                        pacientes[i].Id,

                    Mes =
                        DateTime.Today.Month,

                    Anio =
                        DateTime.Today.Year,

                    Monto =
                        monto,

                    FechaGeneracion =
                        cobro.FechaCobro,

                    Estado =
                        estado,

                    CobroId =
                        cobro.Id
                };

                context.CuotasMensuales.Add(cuota);

                if (estado == "Parcial")
                {
                    context.Pagos.Add(
                        new Pago
                        {
                            CobroId =
                                cobro.Id,

                            FechaPago =
                                cobro.FechaCobro
                                    .AddDays(2),

                            Monto =
                                monto / 2m,

                            MetodoPago =
                                "Efectivo",

                            Referencia =
                                $"CUO-P-{i + 1:D3}",

                            Observaciones =
                                "Pago parcial de cuota mensual."
                        }
                    );
                }

                if (estado == "Pagado")
                {
                    context.Pagos.Add(
                        new Pago
                        {
                            CobroId =
                                cobro.Id,

                            FechaPago =
                                cobro.FechaCobro
                                    .AddDays(2),

                            Monto =
                                monto,

                            MetodoPago =
                                i % 2 == 0
                                    ? "Efectivo"
                                    : "Transferencia",

                            Referencia =
                                $"CUO-{i + 1:D3}",

                            Observaciones =
                                "Cuota mensual cancelada."
                        }
                    );
                }

                await context.SaveChangesAsync();
            }
        }

        // =========================================================
        // DONACIONES
        // =========================================================

        private static async Task CrearDonacionesAsync(
            AsiloDbContext context)
        {
            var tipos = new[]
            {
                "Empresa nacional",
                "Empresa internacional",
                "Gobierno",
                "Persona particular"
            };

            var nombres = new[]
            {
                "Fundación Esperanza",
                "Comercial Los Altos",
                "Grupo Solidario Guatemala",
                "Municipalidad Local",
                "María Fernanda López",
                "Distribuidora San José",
                "Asociación Manos Unidas",
                "Corporación del Pacífico",
                "Carlos Méndez",
                "Importadora Central",
                "Fundación Vida",
                "Ana Patricia Morales",
                "Servicios Integrales del Sur",
                "Cooperativa Amistad",
                "Juan Antonio Reyes",
                "Alimentos de Guatemala",
                "Grupo Empresarial Maya",
                "Lucía Hernández",
                "Fundación Nuevo Amanecer",
                "Industrias del Sur"
            };

            var donaciones =
                new List<Donacion>();

            for (int i = 0; i < 20; i++)
            {
                donaciones.Add(
                    new Donacion
                    {
                        TipoDonante =
                            tipos[i % tipos.Length],

                        NombreDonante =
                            nombres[i],

                        FechaDonacion =
                            DateTime.Today
                                .AddDays(-(i * 3)),

                        Monto =
                            500m +
                            ((i % 10) * 350m),

                        Descripcion =
                            "Donación destinada al funcionamiento general del asilo."
                    }
                );
            }

            context.Donaciones.AddRange(donaciones);

            await context.SaveChangesAsync();
        }

        // =========================================================
        // GASTOS
        // =========================================================

        private static async Task CrearGastosAsync(
            AsiloDbContext context)
        {
            var categorias = new[]
            {
                "Luz",
                "Agua",
                "Alimentación",
                "Mantenimiento",
                "Limpieza",
                "Transporte",
                "Medicamentos",
                "Gas",
                "Internet",
                "Reparaciones"
            };

            var gastos =
                new List<Gasto>();

            for (int i = 0; i < 20; i++)
            {
                var categoria =
                    categorias[i % categorias.Length];

                gastos.Add(
                    new Gasto
                    {
                        Categoria =
                            categoria,

                        Descripcion =
                            $"Pago correspondiente a {categoria.ToLower()}.",

                        FechaGasto =
                            DateTime.Today
                                .AddDays(-(i * 2)),

                        Monto =
                            150m +
                            ((i % 8) * 125m),

                        Proveedor =
                            $"Proveedor {categoria}",

                        Referencia =
                            $"GAS-{i + 1:D4}"
                    }
                );
            }

            context.Gastos.AddRange(gastos);

            await context.SaveChangesAsync();
        }

        // =========================================================
        // PAGOS A FUNDACIÓN
        // =========================================================

        private static async Task CrearPagosFundacionAsync(
            AsiloDbContext context)
        {
            var pagos =
                new List<PagoFundacion>();

            for (int i = 0; i < 20; i++)
            {
                pagos.Add(
                    new PagoFundacion
                    {
                        FechaPago =
                            DateTime.Today
                                .AddDays(-(i + 1)),

                        Monto =
                            50m,

                        MetodoPago =
                            i % 2 == 0
                                ? "Transferencia"
                                : "Cheque",

                        Referencia =
                            $"FUN-{i + 1:D4}",

                        Observaciones =
                            "Pago parcial de obligaciones con Fundación."
                    }
                );
            }

            context.PagosFundacion.AddRange(pagos);

            await context.SaveChangesAsync();
        }
    }
}