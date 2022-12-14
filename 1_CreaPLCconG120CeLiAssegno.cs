using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Siemens.Engineering;
using Siemens.Engineering.Hmi;

using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;

namespace ConsoleAppOraz
{
    class Program
    {
        static void Main(string[] args)
        {
            //Creazione progetto
            IList<TiaPortalProcess> mieiProcessiTIAAperti = TiaPortal.GetProcesses();
            TiaPortalProcess mioProcessoTIAPortal = mieiProcessiTIAAperti[0];
            TiaPortal mioTIAPortalAperto = mioProcessoTIAPortal.Attach();
            Project mioProgettoTIA = mioTIAPortalAperto.Projects[0];
            Console.WriteLine("Progetto creato");

            // Inserimento PLC di Linea
            Device miaStazionePLCLinea = mioProgettoTIA.Devices.CreateWithItem("OrderNumber:6ES7 513-1FL02-0AB0/V2.9", "PLC_Linea", "PLC_Linea");
            //Inserimento del G120C di linea
            Device miaStazioneDriveG120Linea = mioProgettoTIA.Devices.CreateWithItem("OrderNumber:6SL3244-0BB12-1FA0/4.7.13", "Drive_Linea", "Drive_Linea");
            Console.WriteLine("PLC e drive di linea inseriti");
            //Aggiunta PM al drive
            miaStazioneDriveG120Linea.DeviceItems[1].PlugNew("OrderNumber:6SL3224-0BE13-7UA0/-", "PM_G120", 3);
            Console.WriteLine("PM inserito sul drive G120");

            //Impostazione dell'indirizzo IP del PLC
            DeviceItem mioPlcLinea = miaStazionePLCLinea.DeviceItems[1];
            DeviceItem mieInterfaccePLCLinea = mioPlcLinea.DeviceItems[3];
            NetworkInterface mieiDatiInterfaccePLCLinea = mieInterfaccePLCLinea.GetService<NetworkInterface>();
            Node mio_X1_PLCLinea = mieiDatiInterfaccePLCLinea.Nodes[0];
            mio_X1_PLCLinea.SetAttribute("Address", "192.168.0.10");
            Console.WriteLine("IP a PLC assegnato");

            //Modifica Indirizzo IP G120C
            DeviceItem mioDriveG120Linea = miaStazioneDriveG120Linea.DeviceItems[1];
            DeviceItem mieInterfacceG120Linea = mioDriveG120Linea.DeviceItems[0];
            NetworkInterface mieiDatiInterfacceG120Linea = mieInterfacceG120Linea.GetService<NetworkInterface>();
            Node mio_X1_G120Linea = mieiDatiInterfacceG120Linea.Nodes[0];
            mio_X1_G120Linea.SetAttribute("Address", "192.168.0.11");
            Console.WriteLine("IP a drive assegnato");

            //Creo rete PN su controller
            Subnet miaSottoretePN = mioProgettoTIA.Subnets.Create("System:Subnet.Ethernet", "PN/IE");

            //Collego entrambi alla rete appena creata
            mieiDatiInterfaccePLCLinea.Nodes[0].ConnectToSubnet(miaSottoretePN);
            mieiDatiInterfacceG120Linea.Nodes[0].ConnectToSubnet(miaSottoretePN);

            //Creo il sistema profinet per il controllore	
            IoSystem mioSistemaPNLinea = mieiDatiInterfaccePLCLinea.IoControllers.First().CreateIoSystem("PNIO");

            //Connetto il drive alla sottorete PROFINET appena creata e quindi al PLC
            mieiDatiInterfacceG120Linea.IoConnectors[0].ConnectToIoSystem(mioSistemaPNLinea);
            Console.WriteLine("Drive agganciato a PLC");

            mioProgettoTIA.Save();

        }
    }
}
