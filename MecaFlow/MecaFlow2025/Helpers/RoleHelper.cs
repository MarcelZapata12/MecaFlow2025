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
                "Empleado" => module is "Asistencias" or "Diagnosticos" or "Vehiculos" or "Pagos",
                "Cliente" => module is "Diagnosticos" or "Vehiculos",
                _ => false
            };
        }
    }
}