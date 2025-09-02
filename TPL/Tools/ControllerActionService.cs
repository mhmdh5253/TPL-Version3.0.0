using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace TPLWeb.Tools
{
    public class ControllerActionService
    {
        public List<ControllerActions> GetAllControllerActions()
        {
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .Select(type => new ControllerActions
                {
                    ControllerName = type.Name.Replace("Controller", ""),
                    Actions = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                        .Where(m => !m.GetCustomAttributes(typeof(NonActionAttribute)).Any())
                        .Select(m => m.Name)
                        .ToList()
                })
                .ToList();
            return controllers;
        }
    }

    public class ControllerActions
    {
        public string? ControllerName { get; set; }
        public List<string>? Actions { get; set; }
    }
}