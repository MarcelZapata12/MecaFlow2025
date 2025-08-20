namespace MecaFlow2025.Helpers
{
    public static class RoleHelper
    {
        public static bool IsAdmin(HttpContext context)
        {
            return context.Session.GetString("UserRole") == "Administrador";
        }

        public static bool IsEmployee(HttpContext context)
        {
            return context.Session.GetString("UserRole") == "Empleado";
        }

        public static bool IsClient(HttpContext context)
        {
            return context.Session.GetString("UserRole") == "Cliente";
        }

        public static bool HasAccess(HttpContext context, string module)
        {
            var role = context.Session.GetString("UserRole");

            return role switch
            {
                "Administrador" => true,
                "Empleado" => module is "Asistencias" or "Diagnosticos" or "Vehiculos" or "Pagos" or "Facturas",
                "Cliente" => module is "Diagnosticos" or "Vehiculos" or "Facturas",
                _ => false
            };
        }

        // Nuevo método para verificar permisos específicos de acciones
        public static bool CanPerformAction(HttpContext context, string module, string action)
        {
            var role = context.Session.GetString("UserRole");

            return (role, module, action) switch
            {
                // Administradores pueden hacer todo
                ("Administrador", _, _) => true,

                // Empleados pueden hacer CRUD completo en sus módulos
                ("Empleado", "Asistencias", _) => true,
                ("Empleado", "Diagnosticos", _) => true,
                ("Empleado", "Vehiculos", _) => true,
                ("Empleado", "Pagos", _) => true,
                ("Empleado", "Facturas", _) => true,

                // Clientes solo pueden ver (Index, Details) en sus módulos
                ("Cliente", "Diagnosticos", "Index" or "Details") => true,
                ("Cliente", "Vehiculos", "Index" or "Details") => true,
                ("Cliente", "Facturas", "Index" or "Details") => true,

                // Todo lo demás está denegado
                _ => false
            };
        }

        // Método para verificar si puede crear/editar/eliminar
        public static bool CanModify(HttpContext context, string module)
        {
            var role = context.Session.GetString("UserRole");

            return role switch
            {
                "Administrador" => true,
                "Empleado" => module is "Asistencias" or "Diagnosticos" or "Vehiculos" or "Pagos" or "Facturas",
                "Cliente" => false, // Los clientes no pueden modificar nada
                _ => false
            };
        }

        // Método para verificar si puede ver solo sus propios registros
        public static bool ShouldFilterByUser(HttpContext context, string module)
        {
            var role = context.Session.GetString("UserRole");

            return role == "Cliente" && (module == "Vehiculos" || module == "Diagnosticos" || module == "Facturas");
        }
    }
}