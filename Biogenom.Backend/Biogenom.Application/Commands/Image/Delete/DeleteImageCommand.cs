using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands.Image.Delete
{
    public class DeleteImageCommand : IRequest<Unit>
    {
        public Guid ImageId { get; set; }
    }
}
