using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Data.Entity;

namespace ConsoleApp1
{
    public class Global
    {
        public static int numerochiamate = 0;
        public static int valorenumchiamate
        {
            get { return numerochiamate; }
            set { numerochiamate = value; }
        }

        class Program
        {
            //prendo le credenziali dal App.config
            private static string bufferBiella = ConfigurationManager.AppSettings.Get("bufferBiella").ToString();              
            private static string bufferTorino = ConfigurationManager.AppSettings.Get("bufferTorino").ToString();

            public static string[] credentials = { bufferBiella, bufferTorino };

            static void Main(string[] args)
            {
                bool esito;
                //ciclo per ogni credenziale
                Stopwatch sw = new Stopwatch();
                sw.Start();
                foreach (string s in credentials)
                {
                    //splitto le credenziali come user e pass
                    string[] split = s.Split(new char[] { ';' });
                    AggiornamentoDatiNGI agg = new AggiornamentoDatiNGI();
                    //inserisco tutti i nuovi interventi per le credenziali
                    esito = agg.UpdateDatiNGI(split[0], split[1]);
                }
                sw.Stop();
                Console.WriteLine("\n\nTempo di esecuzione : {0}", sw.Elapsed);
                Console.WriteLine("Premere un tasto per uscire...");
                Console.ReadLine();
            }
        }
    }
    //utilizzata per inserire nel database tutti i nuovi interventi
    public class AggiornamentoDatiNGI
    {
        public bool UpdateDatiNGI(string login,string secretCode)
        {
            //ciclo su ogni tipo tipoWS 
            string[] listaTipiWS = { "AR.INST.GET.ELENCO.GST", "AR.INST.GET.ELENCO.ATT", "AR.INST.GET.ELENCO.SOP", "AR.INST.GET.ELENCO.RIT" };

            foreach (string ws in listaTipiWS)
            {

                // recupero da NGI il contenuto di AR.INST.GET.ELENCO.ATT
                string xml = String.Empty;
                string odl = String.Empty;

                bool esito1 = RecuperaXML(ws, login, secretCode,null, out xml);

                bool esito2 = InserisciXML(ws, xml, login, secretCode);
            }

            return true;
        }
        private bool RecuperaXML(string ws, string login, string secretCode,string odl,out string xmlRecuperato)
        {
            xmlRecuperato = String.Empty;
            string ambiente = "http://dwapi.eolo.it/api-service.asp";
            //creo una stringa con il formato dato dalle specifiche
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000");
            // creo la stringa da criptare come da specifiche
            string daCriptare = "timeStamp=" + timestamp + "&wslogin=" + login + secretCode;

            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(daCriptare));
            //cripto la stringa in esadecimale
            StringBuilder sOutput = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sOutput.Append(data[i].ToString("X2"));
            }

            // creo il messaggio XML da inviare
            string postData = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                              "<serviceQuery>" +
                              "<timeStamp>" + timestamp + "</timeStamp>" +
                              "<WSLogin>" + login + "</WSLogin>" +
                              "<mac>" + sOutput.ToString().ToUpper() + "</mac>" +
                              "<rev>01</rev>" +
                              "<dati>" +
                              "<queryType>" + ws + "</queryType>";
            //inserisco dati a seconda del tipo dell'intervento
            switch(ws)
            {
                case "AR.INST.GET.ODL.DETAIL":
                case "AR.INST.GET.LOG.DETAIL":
                    postData += "<odl>" + odl + "</odl>";
                    break;

                default:
                    break;
            }
            //chiudo correttamente il messaggio XML
            postData += "</dati>" + "</serviceQuery>";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            WebRequest request = WebRequest.Create(ambiente);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            try
            {
                reader.Close();
                dataStream.Close();
                response.Close();

                xmlRecuperato = responseFromServer;
                Global.numerochiamate++;
                return true;
            }
            catch(WebException e)
            {
                return false;
            }
        }
        private bool InserisciXML(string ws, string xml,string login, string secretCode)
        {
            try
            {
                //utilizzo xdocument per leggere il documento xml
                XDocument elencoRecord;
                using (StringReader s = new StringReader(xml))
                {
                    elencoRecord = XDocument.Load(s);
                }

                // dichiarazione elenco campi 
                string itemKeyRec = String.Empty;
                DateTime dataLettaRec;
                string dataDaScrivereRec = String.Empty;
                string slaRec = String.Empty;
                string clienteRec = String.Empty;
                string indirizzoRec = String.Empty;
                string capRec = String.Empty;
                string comuneRec = String.Empty;
                string pvRec = String.Empty;
                string contattoRec = String.Empty;
                string telRec = String.Empty;
                string celRec = String.Empty;
                string emailRec = String.Empty;
                decimal latRec;
                decimal longRec;
                DateTime dataAppLettaRec = new DateTime(1920,1,1);
                string dataAppDaScrivereRec = String.Empty;
                bool salvaDataApp = false;
                string myvar = "0";

                // impostazione nome campo chiave
                string nomeCampoKey = String.Empty;
                nomeCampoKey = "ODL";
                //inserisco tutti i campi necessari per ogni item
                foreach (var rec in elencoRecord.Descendants("item"))
                {
                    itemKeyRec = rec.Attribute(nomeCampoKey).Value;
                    dataLettaRec = DateTime.ParseExact(rec.Attribute("data").Value, "dd/MM/yy", CultureInfo.InvariantCulture);

                    if (dataLettaRec == DateTime.Now.Date || dataLettaRec >= DateTime.Now.Date.AddDays(-3))
                    {
                        dataLettaRec = DateTime.ParseExact(rec.Attribute("data").Value, "dd/MM/yy", CultureInfo.InvariantCulture);
                        dataDaScrivereRec = dataLettaRec.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        if (ws == "AR.INST.GET.ELENCO.ATT" || ws == "AR.INST.GET.ELENCO.GST")
                        {
                            slaRec = rec.Attribute("sla").Value;
                        }
                        clienteRec = rec.Attribute("cliente").Value.Replace("'", " ").Replace("\\", " ");
                        indirizzoRec = rec.Attribute("indirizzo").Value.Replace("\'", "\''").Replace("\\", " ");


                        capRec = rec.Attribute("cap").Value;
                        comuneRec = rec.Attribute("comune").Value.Replace("\\", " ");
                        if (comuneRec.Contains("\'"))
                        {
                            comuneRec = comuneRec.Replace("\'", "\''");
                        }

                        pvRec = rec.Attribute("pv").Value;
                        myvar = (rec.Attribute("myVar").Value == "A") ? "1" : "0";
                        contattoRec = rec.Attribute("contatto").Value.Replace("'", " ").Replace("\\", " "); ;
                        telRec = rec.Attribute("tel").Value;
                        celRec = rec.Attribute("cel").Value;
                        emailRec = rec.Attribute("email").Value.Replace("'", " ").Replace("\\", " ");

                        if (rec.Attribute("lat").Value.ToString() != String.Empty)
                        {
                            latRec = System.Convert.ToDecimal(rec.Attribute("lat").Value.ToString(), (CultureInfo.CreateSpecificCulture("en-US")));
                        }
                        else latRec = 0;
                        if (rec.Attribute("long").Value.ToString() != String.Empty)
                        {
                            longRec = System.Convert.ToDecimal(rec.Attribute("long").Value.ToString(), (CultureInfo.CreateSpecificCulture("en-US")));
                        }
                        else longRec = 0;
                        try
                        {
                            dataAppLettaRec = DateTime.ParseExact(rec.Attribute("dataAppuntamento").Value.ToString(), "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);
                            dataAppDaScrivereRec = dataAppLettaRec.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            salvaDataApp = true;
                        }
                        catch (Exception e)
                        {
                            salvaDataApp = false;
                        }
                        //a seconda del tipo di intervento vado ad inserirlo nella tabella corrispondente nel database
                        using (DATI_NGIEntities db = new DATI_NGIEntities())
                        {
                            GetElenco elenco = new GetElenco();

                            //inserisco i dati della nuova tupla
                            elenco.itemKEY = itemKeyRec;
                            elenco.data = dataLettaRec;
                            elenco.sla = slaRec;
                            elenco.cliente = clienteRec;
                            elenco.indirizzo = indirizzoRec;
                            elenco.cap = capRec;
                            elenco.comune = comuneRec;
                            elenco.pv = pvRec;
                            elenco.contatto = contattoRec;
                            elenco.tel = telRec;
                            elenco.cel = celRec;
                            elenco.email = emailRec;
                            elenco.lat = latRec;
                            elenco.@long = longRec;

                            if (dataAppLettaRec.Year == 1920)
                                elenco.dataAppuntamento = null;
                            else
                                elenco.dataAppuntamento = dataAppLettaRec;

                            switch (ws)
                            {
                                case "AR.INST.GET.ELENCO.ATT":
                                    elenco.tipoWS = "ATT";
                                    break;

                                case "AR.INST.GET.ELENCO.GST":
                                    elenco.tipoWS = "GST";
                                    break;

                                case "AR.INST.GET.ELENCO.SOP":
                                    elenco.tipoWS = "SOP";
                                    break;

                                case "AR.INST.GET.ELENCO.RIT":
                                    elenco.tipoWS = "RIT";
                                    break;

                                default:
                                    break;
                            }
                            elenco.login = login;
                            elenco.dataCreazione = DateTime.Today;
                            elenco.dataModifica = null;
                            
                            try
                            {
                                //inserisco il nuovo modello nella tabella GetElencoAtt
                                var d = db.GetElenco.Find(elenco.itemKEY);
                                if (d == null)
                                {
                                    db.GetElenco.Add(elenco);
                                    Console.WriteLine("INSERISCO  {0} - {1}", elenco.itemKEY, elenco.cliente);
                                }
                                else
                                {

                                    //recupero i dati mancanti per andare in update
                                    string xmlUpdate = string.Empty;
                                    string type = "AR.INST.GET.ODL.DETAIL";

                                    bool risultato = RecuperaXML(type, login, secretCode, elenco.itemKEY, out xmlUpdate);

                                    XDocument record;
                                    using (StringReader s = new StringReader(xmlUpdate))
                                    {
                                        record = XDocument.Load(s);
                                    }
                                    foreach (var r in record.Descendants("Cliente"))
                                    {
                                        elenco.cap = r.Attribute("cap").Value;
                                    }
                                    foreach (var r in record.Descendants("Contatto"))
                                    {
                                        elenco.tel = r.Attribute("telefono").Value;
                                        elenco.cel = r.Attribute("cellulare").Value;
                                        elenco.email = r.Attribute("email").Value;
                                    }

                                    db.Entry(d).CurrentValues.SetValues(elenco);
                                    Console.WriteLine("AGGIORNO  {0} - {1}", elenco.itemKEY, elenco.cliente);
                                }
                                //convalido i cambiamenti
                                db.SaveChanges();                              
                            }
                            catch(Exception e)
                            {
                                    db.GetElenco.Remove(elenco);
                                    db.SaveChanges();               
                            }
                        }
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
