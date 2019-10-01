using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data;

namespace RX.Nyss.Web.Controllers
{
    [Route("api/nationalSocieties")]
    public class NationalSocietyController : Controller
    {
        private readonly NyssContext _context;

        public NationalSocietyController(NyssContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllNationalSocieties()
        {
            return Ok(_context.NationalSocieties.ToList());
        }

        [HttpPost]
        public async Task<ActionResult> CreateNationalSociety([FromBody] NationalSociety ns)
        {
            _context.NationalSocieties.Add(ns);

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
