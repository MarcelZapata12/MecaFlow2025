using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MecaFlow2025.Services
{
    public class ChatbotService
    {
        private readonly Dictionary<string, List<string>> _knowledgeBase;
        private readonly Dictionary<string, string> _quickResponses;
        private readonly List<string> _greetings;
        private readonly List<string> _farewells;

        public ChatbotService()
        {
            _greetings = new List<string> { "hola", "buenos días", "buenas tardes", "buenas noches", "hey", "saludos" };
            _farewells = new List<string> { "gracias", "bye", "adiós", "chao", "hasta luego", "nos vemos" };

            _knowledgeBase = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["aceite"] = new List<string> {
                    "🔧 **Cambio de aceite**: Se recomienda cada 5,000-7,500 km o cada 6 meses. Para motores nuevos usa aceite sintético 5W-30 o 5W-40.",
                    "⚠️ **Señales de cambio**: Color oscuro o negro, textura espesa, olor a quemado, nivel bajo frecuente, o ruidos extraños del motor.",
                    "💡 **Función del aceite**: Lubrica piezas internas, reduce fricción, disipa calor y limpia residuos. ¡No retrases el cambio!",
                    "🛢️ **Tipos de aceite**: Sintético (mejor protección), semi-sintético (balance precio-calidad), convencional (económico para autos antiguos)."
                },
                ["frenos"] = new List<string> {
                    "🚗 **Pastillas de freno**: Duran 40,000-70,000 km. Señales de desgaste: chirridos metálicos, vibración al frenar, pedal esponjoso.",
                    "🔴 **Líquido de frenos**: Cambiar cada 2 años. Si el pedal está blando o va hasta el fondo, necesita revisión URGENTE.",
                    "⚠️ **Ruido metálico**: Indica pastillas completamente gastadas rayando los discos. ¡Para inmediatamente y revisa!",
                    "🛑 **Emergencia**: Si pierdes frenos, usa freno de mano progresivamente, busca superficie rugosa, y apaga motor en neutro."
                },
                ["batería"] = new List<string> {
                    "🔋 **Señales de batería débil**: Arranque lento, luces tenues, necesidad de puenteo frecuente, terminales corroídos blancos/verdosos.",
                    "⏰ **Vida útil**: 3-5 años promedio. En climas fríos pierde hasta 35% de potencia. Revisar cada año después del 3er año.",
                    "🧪 **Prueba casera**: Con motor apagado, enciende luces y trata de arrancar. Si las luces se apagan mucho, batería débil.",
                    "🔧 **Mantenimiento**: Limpiar terminales con bicarbonato, verificar nivel de agua (si no es sellada), asegurar sujeción firme."
                },
                ["neumáticos"] = new List<string> {
                    "📏 **Presión**: Revisar mensualmente según manual (usualmente 30-35 PSI). Baja presión = mayor consumo + desgaste irregular.",
                    "🔄 **Rotación**: Cada 10,000 km para desgaste parejo. Profundidad mínima legal 1.6mm, cambiar antes de 3mm para seguridad.",
                    "⚖️ **Balanceo y alineación**: Si vibra el volante = desbalanceo. Si el auto se va a un lado = desalineación. Revisar cada 20,000 km.",
                    "🕳️ **Prueba de la moneda**: Inserta moneda en ranura. Si ves toda la cabeza, es hora de cambiar neumáticos."
                },
                ["motor"] = new List<string> {
                    "⚠️ **Check Engine**: Desde tapa de gasolina floja hasta fallas graves. Escanear códigos OBD2 para diagnóstico preciso.",
                    "🔊 **Ruidos anormales**: Golpeteos (baja presión aceite), silbidos (fuga aire), chirridos (correas). ¡Revisión inmediata!",
                    "💨 **Humo del escape**: Azul (quema aceite), blanco (refrigerante), negro (mezcla rica combustible). Todos requieren atención.",
                    "🌡️ **Sobrecalentamiento**: Parar inmediatamente, no abrir radiador caliente, verificar nivel refrigerante cuando enfríe."
                },
                ["transmisión"] = new List<string> {
                    "⚙️ **Transmisión automática**: Cambiar aceite cada 60,000-100,000 km. Señales: cambios bruscos, resbalones, ruidos.",
                    "🔧 **Transmisión manual**: Aceite cada 50,000-80,000 km. Problemas: dificultad cambios, ruidos al engranar, embrague patina.",
                    "🚨 **Síntomas urgentes**: No entra ningún cambio, patinazos, ruidos fuertes, olor a quemado. ¡No manejar hasta revisar!"
                },
                ["aire_acondicionado"] = new List<string> {
                    "❄️ **Mantenimiento AC**: Cambiar filtro cabina cada 15,000 km, recargar gas cada 2-3 años, limpiar condensador.",
                    "🌡️ **No enfría**: Puede ser gas bajo, compresor dañado, filtro sucio, o fuga en sistema. Revisión profesional necesaria.",
                    "💡 **Consejos**: Usar AC 10 min cada semana (incluso en invierno) para mantener sellos lubricados."
                }
            };

            _quickResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["horario"] = "🕐 **Horarios de MecaFlow**:\n• Lunes a Viernes: 8:00 AM - 6:00 PM\n• Sábados: 9:00 AM - 2:00 PM\n• Domingos: Cerrado",
                ["contacto"] = "📞 **Contacto MecaFlow**:\n• Teléfono: 555-1234\n• Email: info@mecaflow.com\n• Dirección: Av. Principal #123\n• WhatsApp: 555-1234",
                ["cita"] = "📅 **Agendar cita**:\n1️⃣ Llamar: 555-1234\n2️⃣ WhatsApp: 555-1234\n3️⃣ Presencial: Av. Principal #123\n4️⃣ Email: info@mecaflow.com",
                ["precio"] = "💰 **Precios aproximados** (pueden variar):\n• Cambio aceite: $25-40\n• Pastillas frenos: $60-120\n• Batería nueva: $80-150\n• Alineación: $25-35\n*Precios sujetos a cambios",
                ["emergencia"] = "🚨 **Emergencia mecánica**:\n1️⃣ Detente en lugar seguro\n2️⃣ Enciende luces de emergencia\n3️⃣ Llama: 555-1234\n4️⃣ No fuerces el vehículo"
            };
        }

        public string GetResponse(string userInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userInput))
                    return "Por favor, escribe tu pregunta sobre mecánica automotriz. 🔧";

                // Limpiar y normalizar entrada
                userInput = CleanInput(userInput);

                // 1. Saludos
                if (ContainsAnyKeyword(userInput, _greetings))
                {
                    return "¡Hola! 👋 Soy el asistente mecánico de MecaFlow. ¿En qué puedo ayudarte hoy?\n\n" +
                           "Puedo responder sobre:\n" +
                           "🔧 Aceite y lubricantes\n" +
                           "🚗 Sistema de frenos\n" +
                           "🔋 Batería y sistema eléctrico\n" +
                           "🛞 Neumáticos y alineación\n" +
                           "🚙 Problemas del motor\n" +
                           "❄️ Aire acondicionado\n" +
                           "⚙️ Transmisión";
                }

                // 2. Despedidas
                if (ContainsAnyKeyword(userInput, _farewells))
                {
                    return "¡De nada! 😊 Fue un placer ayudarte. Si tienes más preguntas sobre tu vehículo, no dudes en consultarme.\n\n" +
                           "🔧 **MecaFlow - Tu taller de confianza**";
                }

                // 3. Respuestas rápidas específicas
                foreach (var kvp in _quickResponses)
                {
                    if (userInput.Contains(kvp.Key))
                        return kvp.Value;
                }

                // 4. Búsqueda en base de conocimientos
                var matchingTopics = new List<string>();

                foreach (var topic in _knowledgeBase.Keys)
                {
                    if (IsTopicMatch(userInput, topic))
                    {
                        matchingTopics.Add(topic);
                    }
                }

                if (matchingTopics.Any())
                {
                    var topic = matchingTopics.First();
                    var responses = _knowledgeBase[topic];
                    var response = responses[new Random().Next(responses.Count)];

                    return response + $"\n\n💡 ¿Tienes alguna pregunta más específica sobre {topic}?";
                }

                // 5. Búsqueda por palabras clave alternativas
                var alternativeResponse = GetAlternativeResponse(userInput);
                if (!string.IsNullOrEmpty(alternativeResponse))
                    return alternativeResponse;

                // 6. Respuesta por defecto con sugerencias inteligentes
                return GetDefaultResponseWithSuggestions(userInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ChatbotService: {ex.Message}");
                return "Disculpa, estoy teniendo dificultades técnicas. 😅 Por favor intenta con otra pregunta o contacta directamente al taller al 555-1234.";
            }
        }

        private string CleanInput(string input)
        {
            // Remover acentos y caracteres especiales, convertir a minúsculas
            return Regex.Replace(input.ToLower().Trim(), @"[áàäâ]", "a")
                       .Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                       .Replace("ñ", "n");
        }

        private bool ContainsAnyKeyword(string input, List<string> keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsTopicMatch(string input, string topic)
        {
            var topicKeywords = GetTopicKeywords(topic);
            return topicKeywords.Any(keyword => input.Contains(keyword));
        }

        private List<string> GetTopicKeywords(string topic)
        {
            return topic.ToLower() switch
            {
                "aceite" => new List<string> { "aceite", "lubricante", "cambio aceite", "5w30", "sintetico" },
                "frenos" => new List<string> { "freno", "frenas", "pastilla", "disco", "pedal", "chirria", "vibra" },
                "batería" => new List<string> { "bateria", "arranque", "corriente", "terminal", "carga", "puenteo" },
                "neumáticos" => new List<string> { "llanta", "neumatico", "goma", "alineacion", "balanceo", "presion", "desgaste" },
                "motor" => new List<string> { "motor", "check engine", "humo", "ruido", "golpeteo", "calentamiento" },
                "transmisión" => new List<string> { "transmision", "cambio", "embrague", "automatica", "manual", "patina" },
                "aire_acondicionado" => new List<string> { "aire", "ac", "frio", "clima", "aire acondicionado", "calor" },
                _ => new List<string> { topic }
            };
        }

        private string GetAlternativeResponse(string input)
        {
            // Patrones específicos de problemas comunes
            if (Regex.IsMatch(input, @"\b(no arranca|no enciende|no prende)\b"))
                return GetResponseFromTopic("batería") + "\n\n⚠️ También podría ser problema de combustible o starter.";

            if (Regex.IsMatch(input, @"\b(huele|olor)\b.*\b(quemado|aceite)\b"))
                return GetResponseFromTopic("aceite") + "\n\n🚨 Si el olor es muy fuerte, para el vehículo inmediatamente.";

            if (Regex.IsMatch(input, @"\b(vibra|tiembla)\b"))
                return GetResponseFromTopic("neumáticos") + "\n\n También podría ser problema en motor o frenos.";

            if (Regex.IsMatch(input, @"\b(caliente|temperatura|vapor)\b"))
                return GetResponseFromTopic("motor") + "\n\n🌡️ Revisa nivel de refrigerante y termostato.";

            return string.Empty;
        }

        private string GetResponseFromTopic(string topic)
        {
            if (_knowledgeBase.TryGetValue(topic, out var responses))
            {
                return responses[new Random().Next(responses.Count)];
            }
            return $"Para consultas específicas sobre {topic}, te recomiendo contactar directamente con nuestros mecánicos especializados. 📞 555-1234";
        }

        private string GetDefaultResponseWithSuggestions(string input)
        {
            var suggestions = new List<string>
            {
                "🔧 \"¿Cada cuánto cambio el aceite?\"",
                "🚗 \"Mi auto hace ruido al frenar\"",
                "🔋 \"¿Cómo saber si mi batería está mala?\"",
                "🛞 \"Qué presión deben tener mis llantas\"",
                "❄️ \"El aire acondicionado no enfría\"",
                "⚙️ \"La transmisión hace ruidos extraños\""
            };

            var randomSuggestions = suggestions.OrderBy(x => Guid.NewGuid()).Take(3);

            return "🤔 No estoy seguro de entender tu pregunta específica. \n\n" +
                   "Puedo ayudarte con temas como:\n" +
                   "🔧 Mantenimiento preventivo\n" +
                   "⚠️ Diagnóstico de problemas\n" +
                   "📅 Información del taller\n\n" +
                   "**Ejemplos de preguntas:**\n" +
                   string.Join("\n", randomSuggestions) + "\n\n" +
                   "¿Podrías ser más específico con tu consulta? 😊";
        }
    }
}