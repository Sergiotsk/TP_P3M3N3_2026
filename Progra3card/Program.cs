using System;
using MySql.Data.MySqlClient;

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=mi_banco_db;Uid=root;Pwd=;CharSet=utf8mb4;";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nOpción no válida. Intente nuevamente.");
                        Console.ResetColor();
                        PresionarTecla();
                        break;
                }
            }
        }

        // =========================================================================
        // MENÚS
        // =========================================================================

        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   ALTA DE NUEVO CLIENTE Y TARJETA      ");
            Console.WriteLine("========================================");

            Console.WriteLine("\n--- DATOS DEL TITULAR ---");
            Console.WriteLine("Tipo de documento:");
            Console.WriteLine("  1. DNI");
            Console.WriteLine("  2. PASAPORTE");
            Console.Write("Seleccione (1/2): ");
            string opcionDoc = Console.ReadLine() ?? "";
            string tipoDoc = opcionDoc == "2" ? "PASAPORTE" : "DNI";

            Console.Write("Número de documento: ");
            string documento = Console.ReadLine() ?? "";

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine() ?? "";

            Console.Write("Apellido: ");
            string apellido = Console.ReadLine() ?? "";

            Console.Write("Fecha de nacimiento (YYYY-MM-DD): ");
            string fechaNac = Console.ReadLine() ?? "";

            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("\n--- DATOS DE LA TARJETA ---");
            Console.Write("Número de tarjeta (16 dígitos): ");
            string numTarjeta = Console.ReadLine() ?? "";

            Console.WriteLine("Banco emisor:");
            Console.WriteLine("  1. Banco Nación");
            Console.WriteLine("  2. Banco Provincia");
            Console.WriteLine("  3. Banco Galicia");
            Console.WriteLine("  4. Banco Santander");
            Console.WriteLine("  5. Banco BBVA");
            Console.WriteLine("  6. Banco Macro");
            Console.Write("Seleccione un banco (1-6): ");
            string opcionBanco = Console.ReadLine() ?? "";

            string[] bancos = { "Banco Nación", "Banco Provincia", "Banco Galicia", "Banco Santander", "Banco BBVA", "Banco Macro" };

            if (!int.TryParse(opcionBanco, out int indiceBanco) || indiceBanco < 1 || indiceBanco > 6)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nBanco inválido. Operación cancelada.");
                Console.ResetColor();
                PresionarTecla();
                return;
            }

            string bancoEmisor = bancos[indiceBanco - 1];

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    string sqlUsuario = "INSERT INTO usuarios (documento, tipo_doc, nombre, apellido, fecha_nacimiento, email) " +
                                        "VALUES (@documento, @tipo_doc, @nombre, @apellido, @fecha_nacimiento, @email)";

                    using (MySqlCommand cmdUsuario = new MySqlCommand(sqlUsuario, conexion))
                    {
                        cmdUsuario.Parameters.AddWithValue("@documento", documento);
                        cmdUsuario.Parameters.AddWithValue("@tipo_doc", tipoDoc);
                        cmdUsuario.Parameters.AddWithValue("@nombre", nombre);
                        cmdUsuario.Parameters.AddWithValue("@apellido", apellido);
                        cmdUsuario.Parameters.AddWithValue("@fecha_nacimiento", fechaNac);
                        cmdUsuario.Parameters.AddWithValue("@email", email);
                        cmdUsuario.ExecuteNonQuery();
                    }

                    string sqlTarjeta = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, dni_titular) " +
                                        "VALUES (@numero_tarjeta, @banco_emisor, @dni_titular)";

                    using (MySqlCommand cmdTarjeta = new MySqlCommand(sqlTarjeta, conexion))
                    {
                        cmdTarjeta.Parameters.AddWithValue("@numero_tarjeta", numTarjeta);
                        cmdTarjeta.Parameters.AddWithValue("@banco_emisor", bancoEmisor);
                        cmdTarjeta.Parameters.AddWithValue("@dni_titular", documento);

                        int filasAfectadas = cmdTarjeta.ExecuteNonQuery();
                        if (filasAfectadas > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n¡Cliente {nombre} {apellido} registrado y tarjeta emitida correctamente!");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            PresionarTecla();
        }

        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");

            if (Console.ReadLine()?.ToUpper() == "S")
            {
                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nNo se afectó ninguna fila. Verifique el número de cuenta: {numCuenta}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("       EMITIR NUEVA LIQUIDACIÓN         ");
            Console.WriteLine("========================================");

            Console.Write("Número de cuenta de la tarjeta: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.Write("Período (YYYY-MM): ");
            string periodo = Console.ReadLine() ?? "";

            Console.Write("Fecha de vencimiento (YYYY-MM-DD): ");
            string fechaVenc = Console.ReadLine() ?? "";

            Console.Write("Total a pagar: ");
            decimal totalPagar = Convert.ToDecimal(Console.ReadLine());

            Console.Write("Pago mínimo: ");
            decimal pagoMinimo = Convert.ToDecimal(Console.ReadLine());

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    string sql = "INSERT INTO liquidaciones (num_cuenta, periodo, fecha_vencimiento, total_a_pagar, pago_minimo) " +
                                 "VALUES (@num_cuenta, @periodo, @fecha_vencimiento, @total_a_pagar, @pago_minimo)";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@num_cuenta", numCuenta);
                        comando.Parameters.AddWithValue("@periodo", periodo);
                        comando.Parameters.AddWithValue("@fecha_vencimiento", fechaVenc);
                        comando.Parameters.AddWithValue("@total_a_pagar", totalPagar);
                        comando.Parameters.AddWithValue("@pago_minimo", pagoMinimo);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        if (filasAfectadas > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n¡Liquidación del período {periodo} emitida correctamente!");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            PresionarTecla();
        }

        // =========================================================================
        // MÉTODOS DE ACCESO A DATOS
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            string query = "SELECT num_cuenta, numero_tarjeta, banco_emisor, dni_titular FROM tarjetas";

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}",
                                    lector["num_cuenta"].ToString() ?? "",
                                    lector["numero_tarjeta"].ToString() ?? "",
                                    lector["banco_emisor"].ToString() ?? "",
                                    lector["dni_titular"].ToString() ?? "");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            string query = "SELECT u.nombre, u.apellido, u.email, u.documento, u.tipo_doc, " +
                           "t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo " +
                           "FROM tarjetas t " +
                           "INNER JOIN usuarios u ON t.dni_titular = u.documento " +
                           "WHERE t.num_cuenta = @cuenta";

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@cuenta", cuenta);
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("--------------------------------------------------");
                                Console.ResetColor();
                                Console.WriteLine($"Nro. de Cuenta  : {lector["num_cuenta"]}");
                                Console.WriteLine($"Nro. de Tarjeta : {lector["numero_tarjeta"]}");
                                Console.WriteLine($"Banco Emisor    : {lector["banco_emisor"]}");
                                Console.WriteLine($"Estado          : {lector["estado"]}");
                                Console.WriteLine($"Saldo           : $ {lector["saldo"]}");
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("--------------------------------------------------");
                                Console.ResetColor();
                                Console.WriteLine($"Titular         : {lector["nombre"]} {lector["apellido"]}");
                                Console.WriteLine($"Documento       : {lector["tipo_doc"]} {lector["documento"]}");
                                Console.WriteLine($"Email           : {lector["email"]}");
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("--------------------------------------------------");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"\nNo se encontró ninguna tarjeta con el número de cuenta: {cuenta}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            string query = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@cuenta", cuenta);
                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    return false;
                }
            }
        }

        // =========================================================================
        // AUXILIARES
        // =========================================================================

        static void PresionarTecla()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}
