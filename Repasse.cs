using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HMed.Api.Server.Models
{
    public class Repasse
    {
        public double? Valor { get; set; }
        public DateTime? DataProcedimento { get; set; }
        public string DataAno
        {
            get
            {
                return DataProcedimento.HasValue ? string.Format(CultureInfo.GetCultureInfo("pt-BR"), $"{DataProcedimento.Value.ToString("dd")} {DataProcedimento.Value.ToString("MMMM").Substring(0, 3)}") : "";
            }
        }
        public string Procedimento { get; set; }
        public string GrupoProcedimento { get; set; }
        public string Glosa { get; set; }
        public string NomePaciente { get; set; }
        public string PercentualRepasse { get; set; }
        public double? ValorRepasse { get; set; }
        public string Convenio { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public DateTime? DataFiltro { get; set; }
        public string DataCompetenciaFormatada
        {
            get
            {
                return DataFiltro.HasValue ? DataFiltro.Value.ToString("dd/MM/yyyy") : "";
            }
        }
        public string DataRecebimentoFormatado
        {
            get
            {
                if (DataRecebimento.HasValue)
                {
                    return DataRecebimento.Value.ToString("dd/MM/yyyy");
                }
                return string.Empty;
            }
        }
        public string ValorRepasseFormatado
        {
            get
            {
                if (!ValorRepasse.HasValue) return "Indefinido";
                return $"R$ {ValorRepasse.Value:N2}";
            }
        }
        public string ValorFormatado
        {
            get
            {
                if (!Valor.HasValue) return "Indefinido";
                return $"R$ {Valor.Value:N2}";
            }
        }

        public virtual Periodo Periodo
        {
            get
            {
                if (DataFiltro.HasValue)
                    return new Periodo(mes: DataFiltro.Value.Month, ano: DataFiltro.Value.Year);
                else if (DataProcedimento.HasValue)
                {
                    return new Periodo(mes: DataProcedimento.Value.Month, ano: DataProcedimento.Value.Year);
                }
                return new Periodo(0, 0);
            }
        }
        public string Empresa { get; set; }
        public double? ValorGlosa { get; set; }
        public double? GlosaAceita { get; set; }
        public string Justificativa { get; set; }
        public double? ValorRepresentado { get; set; }
        public double? ValorRecebido { get; set; }
        public DateTime? DtRetornoRecurso { get; set; }
        public string valorGlosaFormatado
        {
            get
            {
                if (!ValorGlosa.HasValue) return "Indefinido";
                return $"R$ {ValorGlosa.Value:N2}";
            }
        }
    }

    public class RepasseAReceber : Repasse
    {
        public StatusRepasse Status { get; set; }
        public int Total { get; set; }
        public string DataProcedimentoFormatada
        {
            get
            {
                return DataProcedimento.HasValue ? DataProcedimento.Value.ToString("dd/MM") : "";
            }
        }
        public DateTime? DataAtendimento { get; set; }
        public string DataAtendimentoFormatada
        {
            get
            {
                return DataAtendimento.HasValue ? DataAtendimento.Value.ToString("dd/MM/yyyy") : "";
            }
        }
        public override Periodo Periodo
        {
            get
            {
                if (DataFiltro.HasValue)
                    return new Periodo(mes: DataFiltro.Value.Month, ano: DataFiltro.Value.Year);
                else if (DataProcedimento.HasValue)
                {
                    return new Periodo(mes: DataProcedimento.Value.Month, ano: DataProcedimento.Value.Year);
                }
                return new Periodo(0, 0);
            }
        }
    }

    public class RepasseRecebido : Repasse
    {
        public string DataRecebimentoFormatada
        {
            get
            {
                return DataRecebimento.HasValue ? DataRecebimento.Value.ToString("dd/MM") : "";
            }
        }
    }

    public class StatusRepasse
    {
        public long IdStatus { get; set; }
        public char Chave { get; set; }
        public string Descricao { get; set; }
        public bool Perdido { get; set; }
        public string Cor { get; set; }
        public int Passo { get; set; }
    }

    public struct Periodo
    {
        public Periodo(int ano, int mes)
        {
            Ano = ano;
            Mes = mes;
        }

        public int Ano { get; set; }
        public int Mes { get; set; }

        public string Descricao
        {
            get
            {
                var info = CultureInfo.GetCultures(CultureTypes.AllCultures).Single(p => p.Name == "pt-BR");
                var data = info.DateTimeFormat.GetMonthName(Mes).ToCharArray();
                data[0] = data[0].ToString().ToUpper()[0];
                var dataS = new String(data);
                return $"{dataS}/{Ano}";
            }
        }
    }

    public class PeriodoDeRepasseRecebido
    {
        public IList<RepasseRecebido> RepasseRecebido { get; set; }
        public double ValorTotalRecebido => RepasseRecebido.Sum(p =>
        {
            if (p.ValorRepasse.HasValue)
                return p.ValorRepasse.Value;
            else
                return 0;
        });
        public string ValorFormatadoRecebido
        {
            get
            {
                return $"R${ValorTotalRecebido:N2}";
            }
        }
        public string DescricaoRecebido
        {
            get { return $"{Periodo.Descricao} : {ValorFormatadoRecebido}"; }
        }
        public Periodo Periodo { get; set; }
    }

    public class PeriodoDeRepasseAReceber
    {
        public IList<RepasseAReceber> RepasseAReceber { get; set; }
        public double ValorTotal => RepasseAReceber.Sum(p =>
        {
            if (p.ValorRepasse.HasValue)
                return p.ValorRepasse.Value;
            else
                return 0;
        });
        public string ValorFormatado
        {
            get
            {
                if (ValorTotal == 0)
                    return "";
                return $"R${ValorTotal:N2}";
            }
        }
        public string Descricao
        {
            get 
            {
                if (ValorTotal == 0)
                    return $"{Periodo.Descricao}";
                return $"{Periodo.Descricao} : {ValorFormatado}"; 
            }
        }
        public Periodo Periodo { get; set; }
    }
    public class RetornoRepasse
    {
        public IList<RepasseRecebido> ListaRepasses { get; set; }
        public IList<string> ListaGrupoFiltro { get; set; }
    }

    public class RetornoPeriodoDeRepasseRecebido
    {
        public IList<PeriodoDeRepasseRecebido> ListaPeriodoDeRepasseRecebido { get; set; }
        public IList<string> ListaGrupoFiltro { get; set; }
    }

    public class RetornoAReceber
    {
        public IList<RepasseAReceber> ListaAReceber { get; set; }
        public IList<string> ListaGrupoFiltro { get; set; }
    }
    public class RetornoPeriodoDeRepasseAReceber
    {
        public IList<PeriodoDeRepasseAReceber> ListaPeriodoDeRepasseAReceber { get; set; }
        public IList<string> ListaGrupoFiltro { get; set; }
    }
    public class RepasseRecibo
    {
        public long idpagamento { get; set; }
        public double? valor { get; set; }
        public string competencia { get; set; }
        public string descricao { get; set; }
        public string nome { get; set; }
        public long CPF { get; set; }
        public string CnpjFornecedor { get; set; }
        public double? desconto { get; set; }
        public double? desconto_rp { get; set; }
        public long idrepasse { get; set; }
        public long empresa { get; set; }
    }
    public class RepasseImposto
    {
        public long iddetalhamento { get; set; }
        public double valoraliquota { get; set; }
        public string descricao { get; set; }
        public double? valordetalhamento { get; set; }
        public long empresa { get; set; }
    }
}