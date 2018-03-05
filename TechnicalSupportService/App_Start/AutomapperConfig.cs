using AutoMapper;
using TechnicalSupportService.Areas;

namespace TechnicalSupportService
{
    public class AutomapperConfig
    {
        public static void Register()
        {
            ConfigureMapping();
        }
        private static void ConfigureMapping()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());
        }
    }
}