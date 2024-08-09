using HMed.Api.Server.Client;
using HMed.Api.Server.Helper;
using HMed.Api.Server.Models;
using HMed.Api.Server.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HMed.Api.Server.Controllers
{
    [Route("api/[controller]/")]
    [Authorize()]
    public class RepasseController : BaseController
    {
        [HttpGet]

        [Route("Recebido")]
        public async Task<IActionResult> Recebido(string Competencia = "", string Convenio = "", string Paciente = "", long? Empresa = null, string grupo = null)
        {
            await RegistraDetalheAcesso(this);

            List<string> filtros = new List<string>();
            if (!string.IsNullOrEmpty(Competencia))
                filtros.Add("Competencia=" + Competencia.Substring(3,7));
            if (!string.IsNullOrEmpty(Convenio))
                filtros.Add("Convenio=" + Convenio);
            if (!string.IsNullOrEmpty(Paciente))
                filtros.Add("Paciente=" + Paciente);
            if (Empresa.IsNotNull())
                filtros.Add("Empresa=" + Empresa.ToString());


            string url;
            if (filtros.Count > 0)
                url = HostCliente + "/api/repasse/recebido?" + string.Join('&', filtros);
            else
                url = HostCliente + "/api/repasse/recebido";

            var response = await HttpRequestFactory.Get(url, Token);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IList<RepasseRecebido> Json;
                Json = JsonConvert.DeserializeObject<IList<RepasseRecebido>>(response.ContentAsString());
                var retornoRepasse = new RetornoRepasse
                {
                    ListaGrupoFiltro = Json.Select(x => x.GrupoProcedimento).Distinct().ToList()
                };
                if (retornoRepasse.ListaGrupoFiltro.Count > 0 && string.IsNullOrEmpty(retornoRepasse.ListaGrupoFiltro[0]))
                    retornoRepasse.ListaGrupoFiltro.RemoveAt(0);

                if (!string.IsNullOrEmpty(grupo)) retornoRepasse.ListaRepasses = Json.Where(x => x.GrupoProcedimento.Equals(grupo)).ToList();
                else retornoRepasse.ListaRepasses = Json;

                var retorno = new List<PeriodoDeRepasseRecebido>();

                var sort = retornoRepasse.ListaRepasses.GroupBy(p => p.Periodo);
                foreach (var p in sort)
                {
                    PeriodoDeRepasseRecebido prr = new PeriodoDeRepasseRecebido
                    {
                        Periodo = new Periodo(p.Key.Ano, p.Key.Mes),
                        RepasseRecebido = p.ToList()
                    };
                    retorno.Add(prr);
                }

                RetornoPeriodoDeRepasseRecebido retornoPeriodoDeRepasseRecebido = new RetornoPeriodoDeRepasseRecebido()
                {
                    ListaGrupoFiltro = retornoRepasse.ListaGrupoFiltro,
                    ListaPeriodoDeRepasseRecebido = retorno
                };

                return Ok(retornoPeriodoDeRepasseRecebido);
            }
            else
                return StatusCode((int)response.StatusCode, response.ContentAsString());
        }

        [HttpGet]
        [Route("AReceber")]
        public async Task<IActionResult> AReceber(string Competencia = "", string Convenio = "", string Paciente = "", long? Empresa = null, string grupo = null)
        {

            await RegistraDetalheAcesso(this);

            List<string> filtros = new List<string>();
            if (!string.IsNullOrEmpty(Competencia))
                filtros.Add("Competencia=" + Competencia.Substring(3, 7));
            if (!string.IsNullOrEmpty(Convenio))
                filtros.Add("Convenio=" + Convenio);
            if (!string.IsNullOrEmpty(Paciente))
                filtros.Add("Paciente=" + Paciente);
            if (Empresa.IsNotNull())
                filtros.Add("Empresa=" + Empresa.ToString());

            string url;
            if (filtros.Count > 0)
                url = HostCliente + "/api/repasse/areceber?" + string.Join('&', filtros);
            else
                url = HostCliente + "/api/repasse/areceber";

            var response = await HttpRequestFactory.Get(url, Token);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IList<RepasseAReceber> Json;
                Json = JsonConvert.DeserializeObject<IList<RepasseAReceber>>(response.ContentAsString());
                var retornoAReceber = new RetornoAReceber();

                if (!string.IsNullOrEmpty(grupo)) retornoAReceber.ListaAReceber = Json.Where(x => x.GrupoProcedimento.Equals(grupo)).ToList();
                else retornoAReceber.ListaAReceber = Json;

                retornoAReceber.ListaGrupoFiltro = Json.Select(x => x.GrupoProcedimento).Distinct().ToList();
                if (retornoAReceber.ListaGrupoFiltro.Count > 0 && string.IsNullOrEmpty(retornoAReceber.ListaGrupoFiltro[0]))
                {
                    retornoAReceber.ListaGrupoFiltro.RemoveAt(0);
                }
                if (!ParametroRepositorio.MostraValorRepasseAReceber(IdHospital))
                {
                    foreach (var repasse in retornoAReceber.ListaAReceber)
                    {
                        repasse.Valor = null;
                        repasse.ValorRepasse = null;
                    }
                }
                var retorno = new List<PeriodoDeRepasseAReceber>();
                var sort = retornoAReceber.ListaAReceber.GroupBy(p => p.Periodo);
                foreach (var p in sort)
                {
                    PeriodoDeRepasseAReceber prr = new PeriodoDeRepasseAReceber
                    {
                        Periodo = new Periodo(p.Key.Ano, p.Key.Mes),
                        RepasseAReceber = p.ToList()
                    };
                    retorno.Add(prr);
                }

                RetornoPeriodoDeRepasseAReceber retornoPeriodoDeRepasseAReceber = new RetornoPeriodoDeRepasseAReceber()
                {
                    ListaGrupoFiltro = retornoAReceber.ListaGrupoFiltro,
                    ListaPeriodoDeRepasseAReceber = retorno
                };

                return Ok(retornoPeriodoDeRepasseAReceber);
            }
            else
                return StatusCode((int)response.StatusCode, response.ContentAsString());
        }

        [HttpGet]
        [Route("ReciboMedico")]
        public async Task<IActionResult> ReciboMedico(string Competencia, long? empresa = null)
        {

            List<string> filtros = new List<string>();
            if (!string.IsNullOrEmpty(Competencia))
                filtros.Add("Competencia=" + Competencia);
            if (ParametroRepositorio.MostraFiltroEmpresaRepasse(IdHospital) == true)
                filtros.Add("Empresa=" + empresa.ToString());

            var responseRecibo = await HttpRequestFactory.Get(HostCliente + "/api/repasse/ReciboMedico?" + string.Join('&', filtros), Token);

            if (responseRecibo.StatusCode != HttpStatusCode.OK)
                return StatusCode((int)responseRecibo.StatusCode, responseRecibo.ContentAsString());

            var responseImposto = await HttpRequestFactory.Get(HostCliente + "/api/repasse/ReciboImpostos?" + string.Join('&', filtros), Token);

            if (responseImposto.StatusCode != HttpStatusCode.OK)
                return StatusCode((int)responseImposto.StatusCode, responseImposto.ContentAsString());

            var responseEmpresa = await HttpRequestFactory.Get(HostCliente + "/api/repasse/BuscaEmpresas", Token);

            if (responseEmpresa.StatusCode != HttpStatusCode.OK)
                return StatusCode((int)responseEmpresa.StatusCode, responseEmpresa.ContentAsString());

            var RetornoEmpresa = JsonConvert.DeserializeObject<IList<Empresa>>(responseEmpresa.ContentAsString());

            var RetornoImposto = JsonConvert.DeserializeObject<IList<RepasseImposto>>(responseImposto.ContentAsString());

            var RetornoRecibo = JsonConvert.DeserializeObject<IList<RepasseRecibo>>(responseRecibo.ContentAsString());

            if (RetornoRecibo.Count() == 0)
                return NoContent();
            
            string html = "";

            var listaIdEmpresa = RetornoRecibo.DistinctBy(x => x.empresa).Select(x => x.empresa);

            var empresasFiltradas = RetornoEmpresa.Where(x=> listaIdEmpresa.Contains(x.Id)).ToList();

            foreach (var _empresa in empresasFiltradas)// inicia foreach paginação
            {

                var recibo = RetornoRecibo.Where(x => x.empresa == _empresa.Id).FirstOrDefault(); //Usa o recibo p/ completar cabeçalho

                html += $"!!<!doctype html>"
                + $"<html xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'>"
                + $"<body>"
                + $"<head style=\"position: absolute; top: 0; width: 100%\">"
                + $"</head>";

                html += $"<div>"
                + $"<p style=\"text-align: center; font-size:11.5px;\"><strong>{_empresa.RazaoSocialEmpresa}</strong></p>"
                + $"<p style=\"text-align: center; font-size:9.6px;\">{_empresa.Endereco}, {_empresa.Numero} - {_empresa.Bairro}</p>"
                + $"<p style=\"text-align: center; font-size:9.6px;\">{_empresa.Cidade} - {_empresa.Uf} CEP: {_empresa.Cep}</p>"
                + $"<p style=\"text-align: center; font-size:9.6px;\">CNPJ: {_empresa.Cnpj} IE: ISENTA</p>"
                + $"<p style=\"text-align: center; font-size:11.5px;\"><strong>Solicitação de Pagamento</strong></p>"
                + $"<p style=\"text-align: left; font-size:9.6px;\">AO CONTAS A PAGAR</p>"
                + $"<p style=\"text-align: left; font-size:9.6px;\">Solicito efetuar o pagamento referente a Conta a pagar n&uacute;mero {recibo.idpagamento}, referente ao repasse de {recibo.competencia}, conforme demonstrativo abaixo:</p>"
                + $"<p style=\"text-align: left;\"></p>"
                + $"<p style=\"text-align: left; font-size:9.6px;\">Prestador: {recibo.nome}</p>"
                + $"<p style=\"text-align: left; font-size:9.6px;\">CPF/CNPJ: {recibo.CPF}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Data Ref. {DateTime.UtcNow.AddHours(-3)}&nbsp; &nbsp; &nbsp; &nbsp;Moeda: R$</p>"
                + $"<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\">"
                + $"<tbody>"
                + $"<tr>"
                + $"<th style=\"font-size:9.6px;\"> Código</th>"
                + $"<th style=\"font-size:9.6px;\"> Descrição </th>"
                + $"<th style=\"font-size:9.6px;\"> Vl Repasse </th>"
                + $"<th style=\"font-size:9.6px;\"> Vl Desconto</th>"
                + $"</tr>";


                string tabela = "";
                double valortotal = 0;
                double descontoTotal = 0;
                
                foreach (var recibor in RetornoRecibo.Where(x=> x.empresa == _empresa.Id))
                {
                    tabela += $"<tr>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> {recibor.idrepasse}</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> {recibor.descricao}</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> R${recibor.valor}</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> R${recibor.desconto.GetValueOrDefault()}</td>"
                            + $"</tr>";
                    valortotal = valortotal + recibor.valor.Value;
                    descontoTotal = descontoTotal + recibor.desconto.GetValueOrDefault();
                }

                html += $"{tabela}"
                + $"</tbody>"
                + $"</table>"
                + $"<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\"> "
                + $"<tbody>"
                + $"<tr>"
                + $"<td style=\"width: 100%; text-align: right; font-size:9.6px;background-color: rgb(216,216,216);\">Total Repasse:   R${valortotal - descontoTotal} &nbsp; &nbsp; &nbsp;&nbsp;</td> "
                + $"</tr>"
                + $"</tbody>"
                + $"</table> ";
                string htmlimposto = "";
                double valorImposto = 0;
                if (RetornoImposto.Count() != 0)
                {
                    string tabelaimposto = "";
                    foreach (var imposto in RetornoImposto.Where(x => x.empresa == _empresa.Id))
                    {
                        tabelaimposto += $"<tr>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> {imposto.iddetalhamento}</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> {imposto.descricao}</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> {imposto.valoraliquota}%</td>"
                                + $"<td style=\"text-align: center; font-size:9.6px;\"> R${imposto.valordetalhamento.GetValueOrDefault()}</td>"
                            + $"</tr>";
                        valorImposto = valorImposto + imposto.valordetalhamento.GetValueOrDefault();
                    }

                    htmlimposto = "<p style=\"text-align: left;\">Detalhamento</p>"
                        + $"<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\">"
                        + $"<tbody>"
                        + $"<tr>"
                        + $"<th style=\"font-size:9.6px;\"> Código</th>"
                        + $"<th style=\"font-size:9.6px;\"> Descrição detalhamento </th>"
                        + $"<th style=\"font-size:9.6px;\"> Aliquota </th>"
                        + $"<th style=\"font-size:9.6px;\"> Vl Imposto</th>"
                        + $"</tr>"
                        + $"{tabelaimposto}"
                        + $"</tbody>"
                        + $"</table>";
                }

                html += $"{htmlimposto}"
                + $"<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\"> "
                + $"<tbody>"
                + $"<tr>"
                + $"<td style=\"width: 100%; text-align: right; font-size:9.6px;\">Valor Liquido:&nbsp; &nbsp; &nbsp; R$ {(valortotal - descontoTotal) - valorImposto}&nbsp; &nbsp; &nbsp;&nbsp;</td> "
                + $"</tr>"
                + $"</tbody>"
                + $"</table> "
                + $"<br></br>"
                + $"<br></br>"
                + $"<br></br>"
                + $"<br></br>"
                + $"<p style=\"text-align: center;\">___________________________________________ </p>"
                + $"<p style=\"text-align: center; font-size:9.6px;\">Assinatura</p> "
                + $"</ div >";

                html +=   $"</body> "
                +$"</html>";
                
            }// fecha foreach // fim pagina

            ArquivosCliente arquivo = new ArquivosCliente()
            {
                Formato = "HTML",
                ArquivoRTF = html
            };
            
            var pdf = Converter.ConverteArquivoPDFRepasse(arquivo, IdHospital, true);
            Arquivo _arquivo = new Arquivo();
            if (pdf.Arquivo.IsNotNull())
            {
                //Efetua o upload no Firebase e retorna o link de download
                _arquivo.URL = await FirebaseHelper.UploadTemporario(pdf.Arquivo, pdf.ArquivoNomeFormato, IdHospital);
                _arquivo.IsURLPDF = true;
            }
            return Ok(_arquivo);
        }

        [HttpGet]
        [Route("BuscaEmpresas")]
        public async Task<IActionResult> BuscaEmpresas()
        {
            var response = await HttpRequestFactory.Get(HostCliente + "/api/Repasse/BuscaEmpresas", Token);

            return TrataResponse<List<Empresa>>(response);
        }
    }
}