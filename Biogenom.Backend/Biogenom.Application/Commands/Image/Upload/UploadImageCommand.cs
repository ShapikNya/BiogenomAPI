using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands.Image.Upload
{
    public class UploadImageCommand : IRequest<Guid>
    {
        public string FileUrl { get; set; }
    }
}
