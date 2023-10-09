using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerivet.Client;
using Newtonsoft.Json.Linq;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;


public class TelerivetSendSmsClient
{
    public async Task SendTelerivetSms()
    {
        TelerivetAPI tr = new TelerivetAPI("_iN2i_VdAJFTIm5BYMzFmkANujPlTyFeENW0");
        Project project = tr.InitProjectById("PJc4c294a93b50ec5c");

        // send message
        Message sent_msg = await project.SendMessageAsync(Util.Options(
            "content", "hello world",
            "to_number", "+4798425349"
            ));
    }
}
