using Biogenom.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Persistence
{
    public class DbInitializer
    {
        public static void Initialize(BiogenomkDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
