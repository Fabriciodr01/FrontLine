using System;
using System.Collections.Generic;

namespace FrontLine.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ??= new ServiceLocator();

        private readonly Dictionary<Type, object> _services = new();

        // Private constructor — use Instance
        private ServiceLocator() { }

        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning(
                    $"[ServiceLocator] Overwriting existing service: {type.Name}");
            }
            _services[type] = service;
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Service not registered: {type.Name}. Did you forget to register it in Bootstrap?");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var found))
            {
                service = (T)found;
                return true;
            }
            service = null;
            return false;
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}
