﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Codice generato da un modello.
//
//     Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//     Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleApp1
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DATI_NGIEntities : DbContext
    {
        public DATI_NGIEntities()
            : base("name=DATI_NGIEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AntenneInstallate> AntenneInstallate { get; set; }
        public virtual DbSet<ElencoBTS> ElencoBTS { get; set; }
        public virtual DbSet<ElencoLinkOdl> ElencoLinkOdl { get; set; }
        public virtual DbSet<EoloCompensi> EoloCompensi { get; set; }
        public virtual DbSet<GetElenco> GetElenco { get; set; }
        public virtual DbSet<GetElencoAntenneInstallate> GetElencoAntenneInstallate { get; set; }
        public virtual DbSet<GetElencoATT> GetElencoATT { get; set; }
        public virtual DbSet<GetElencoBTS> GetElencoBTS { get; set; }
        public virtual DbSet<GetElencoGST> GetElencoGST { get; set; }
        public virtual DbSet<GetElencoKitOdl> GetElencoKitOdl { get; set; }
        public virtual DbSet<GetElencoLinkOdl> GetElencoLinkOdl { get; set; }
        public virtual DbSet<GetElencoLog> GetElencoLog { get; set; }
        public virtual DbSet<GetElencoMagazzinoAntenne> GetElencoMagazzinoAntenne { get; set; }
        public virtual DbSet<GetElencoMagazzinoEolobox> GetElencoMagazzinoEolobox { get; set; }
        public virtual DbSet<GetElencoRIT> GetElencoRIT { get; set; }
        public virtual DbSet<GetElencoSOP> GetElencoSOP { get; set; }
        public virtual DbSet<KitOdl> KitOdl { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<LogEsecuzione> LogEsecuzione { get; set; }
        public virtual DbSet<MagazzinoAntenne> MagazzinoAntenne { get; set; }
        public virtual DbSet<MagazzinoEolobox> MagazzinoEolobox { get; set; }
        public virtual DbSet<Attivazioni_Ordini> Attivazioni_Ordini { get; set; }
    }
}
