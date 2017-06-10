using OpenNETCF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OpenNETCF.DFDS.Test
{
    public interface IDfdsSerializer
    {
        object Deserialize(string serializedString, Type objectType);
        string Serialize(object entity);
    }

    public class DfdsJsonSerializer : IDfdsSerializer
    {
        public object Deserialize(string serializedString, Type objectType)
        {
            return JsonConvert.DeserializeObject(serializedString, objectType);
        }

        public string Serialize(object entity)
        {
            return JsonConvert.SerializeObject(entity);
        }
    }

    public class RestBasedStore : IDfdsRemoteStore
    {
        private RestConnector m_connector;
        private Dictionary<Type, string> m_registeredEndpoints;
        private IDfdsSerializer m_serializer;

        public RestBasedStore(string apiRoot, IDfdsSerializer serializer)
        {
            m_connector = new RestConnector(apiRoot);
            m_serializer = serializer;
            m_registeredEndpoints = new Dictionary<Type, string>();
        }

        public void RegisterEndpoint<T>(string path)
        {
            Validate
                .Begin()
                .ParameterIsNotNullOrWhitespace(path, "path")
                .Check();
                
            var t = typeof(T);
            lock (m_registeredEndpoints)
            {
                if (!m_registeredEndpoints.ContainsKey(t))
                {
                    m_registeredEndpoints.Add(t, path);
                }
            }
        }

        public object[] GetMultiple(Type t, DateTime? lastRequest) 
        {
            string endpoint;

            // NOTE:
            // DFDS will notify the implementation of when the last request was made, but it does no filtering itself.
            // It is up to the store implementation to keep track of "things already pulled" if it wants to minimize traffic.

            string queryParameters = null;
            if (lastRequest.HasValue)
            {
                queryParameters = string.Format("?since={0}", lastRequest.Value.ToString("s"));
            }

            endpoint = GetEndpointForType(t);

            try
            {
                endpoint += queryParameters ?? string.Empty;

                var result = m_connector.GetString(endpoint);
                var arrayType = Array.CreateInstance(t, 0).GetType();

                var objects = m_serializer.Deserialize(result, arrayType);
                return objects as object[];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        public void Insert<T>(T item)
        {
            // TODO: POST
            var t = typeof(T);

            var endpoint = GetEndpointForType(t);
            var payload = m_serializer.Serialize(item);

            m_connector.Post(endpoint, payload);
        }

        private string GetEndpointForType(Type t)
        {
            // do we have a registered endpoint?
            lock (m_registeredEndpoints)
            {
                if (m_registeredEndpoints.ContainsKey(t))
                {
                    return m_registeredEndpoints[t];
                }
                else
                {
                    // default to the class name is nothing is registered
                    return t.Name;
                }
            }
        }
        public void Update<T>(T item)
        {
            // TODO: PUT
        }
    }
}
