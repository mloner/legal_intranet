using System;
using AutoMapper;

namespace Utg.LegalService.API.Configuration
{
    public static class AutoMapperConfig
    {
        public static void ConfigureMappings(IMapperConfigurationExpression config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
        }
	}
}
