using AutoMapper;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Schulungsportal_2.Models.Anmeldungen.AnmeldungRepository;

namespace Schulungsportal_2.Models
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<Anmeldung, AnmeldungWithMatchCount>();
            CreateMap<Schulung, SchulungCreateViewModel>();
            CreateMap<Schulung, SchulungViewModel>();
        }
    }
}
