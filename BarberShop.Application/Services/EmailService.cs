using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BarberShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _sendGridApiKey;
        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;

        public EmailService(string sendGridApiKey, ILogService logService, IConfiguration configuration)
        {
            _sendGridApiKey = sendGridApiKey;
            _logService = logService;
            _configuration = configuration;
        }

        public async Task EnviarEmailAgendamentoAsync(string destinatarioEmail, string destinatarioNome, string assunto, string conteudo, string barbeiroNome, DateTime dataHoraInicio,
            DateTime dataHoraFim, decimal total, string formaPagamento, string nomeBarbearia, string googleCalendarLink = null)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de agendamento para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);

                string htmlContent = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: 'Arial', sans-serif;
                                background-color: #2c2f33;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                background-color: #23272a;
                                color: #ffffff;
                                max-width: 600px;
                                margin: 20px auto;
                                border-radius: 10px;
                                padding: 20px;
                                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                            }}
                            h1 {{
                                font-size: 24px;
                                color: #e74c3c;
                                text-align: center;
                                border-bottom: 2px solid #e74c3c;
                                padding-bottom: 10px;
                                margin-bottom: 20px;
                            }}
                            p {{
                                font-size: 16px;
                                line-height: 1.6;
                                color: #ffffff;
                            }}
                            .details {{
                                background-color: #99aab5;
                                padding: 15px;
                                border-radius: 8px;
                                margin-bottom: 20px;
                                color: #23272a;
                            }}
                            .btn {{
                                background-color: #e74c3c;
                                color: #000000;
                                padding: 10px 15px;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                                display: inline-block;
                            }}
                            .btn:hover {{
                                background-color: #c0392b;
                            }}
                            .footer {{
                                text-align: center;
                                margin-top: 20px;
                                font-size: 12px;
                                color: #99aab5;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Confirmação de Agendamento</h1>
                            <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                            <p>Aqui está o resumo do seu agendamento:</p>
                            <div class='details'>
                                <p><strong>Barbeiro:</strong> {barbeiroNome}</p>
                                <p><strong>Data e Hora de Início:</strong> {dataHoraInicio:dd/MM/yyyy - HH:mm}</p>
                                <p><strong>Data e Hora de Fim:</strong> {dataHoraFim:dd/MM/yyyy - HH:mm}</p>
                                <p><strong>Forma de Pagamento:</strong> {formaPagamento}</p>
                                <p><strong>Valor Total:</strong> R$ {total:F2}</p>
                            </div>";

                if (!string.IsNullOrEmpty(googleCalendarLink))
                {
                    htmlContent += $@"
                            <p style='text-align: center;'>
                                <a href='{googleCalendarLink}' class='btn' style='color: #000000;'>Adicionar ao Google Calendar</a>
                            </p>";
                }

                htmlContent += $@"
                            <p>Obrigado por escolher a {nomeBarbearia}!</p>
                            <div class='footer'>
                                <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailNotificacaoBarbeiroAsync(string barbeiroEmail, string barbeiroNome, string clienteNome, List<string> servicos, DateTime dataHoraInicio, DateTime dataHoraFim, decimal total, string formaPagamento, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de notificação para {barbeiroEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(barbeiroEmail, barbeiroNome);

                string htmlContent = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: 'Arial', sans-serif;
                                    background-color: #2c2f33;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    background-color: #23272a;
                                    color: #ffffff;
                                    max-width: 600px;
                                    margin: 20px auto;
                                    border-radius: 10px;
                                    padding: 20px;
                                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                }}
                                h1 {{
                                    font-size: 24px;
                                    color: #e74c3c;
                                    text-align: center;
                                    border-bottom: 2px solid #e74c3c;
                                    padding-bottom: 10px;
                                    margin-bottom: 20px;
                                }}
                                p {{
                                    font-size: 16px;
                                    line-height: 1.6;
                                    color: #ffffff;
                                }}
                                .details {{
                                    background-color: #99aab5;
                                    padding: 15px;
                                    border-radius: 8px;
                                    margin-bottom: 20px;
                                    color: #23272a;
                                }}
                                .footer {{
                                    text-align: center;
                                    margin-top: 20px;
                                    font-size: 12px;
                                    color: #99aab5;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>Novo Agendamento Recebido</h1>
                                <p>Olá, <strong>{barbeiroNome}</strong>,</p>
                                <p>Um novo agendamento foi realizado. Confira os detalhes:</p>
                                <div class='details'>
                                    <p><strong>Cliente:</strong> {clienteNome}</p>
                                    <p><strong>Data e Hora de Início:</strong> {dataHoraInicio:dd/MM/yyyy - HH:mm}</p>
                                    <p><strong>Data e Hora de Fim:</strong> {dataHoraFim:dd/MM/yyyy - HH:mm}</p>
                                    <p><strong>Forma de Pagamento:</strong> {formaPagamento}</p>
                                    <p><strong>Valor Total:</strong> R$ {total:F2}</p>
                                    <p><strong>Serviços Solicitados:</strong></p>
                                    <ul>";

                foreach (var servico in servicos)
                {
                    htmlContent += $"<li>{servico}</li>";
                }

                htmlContent += $@"
                                    </ul>
                                </div>
                                <p>Por favor, prepare-se para o atendimento.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                var assunto = "Novo Agendamento Confirmado";
                var msg = MailHelper.CreateSingleEmail(from, to, assunto, string.Empty, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de notificação enviado com sucesso para: {barbeiroEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de notificação para {barbeiroEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailCodigoVerificacaoAsync(string destinatarioEmail, string destinatarioNome, string codigoVerificacao, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de verificação para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Seu Código de Verificação";
                var conteudo = $"Olá, {destinatarioNome}!\n\nSeu código de verificação é: {codigoVerificacao}\n\nEste código expira em 5 minutos.";

                string htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            background-color: #2c2f33;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            background-color: #23272a;
                            color: #ffffff;
                            max-width: 600px;
                            margin: 20px auto;
                            border-radius: 10px;
                            padding: 20px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{
                            font-size: 24px;
                            color: #e74c3c;
                            text-align: center;
                            border-bottom: 2px solid #e74c3c;
                            padding-bottom: 10px;
                            margin-bottom: 20px;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #ffffff;
                        }}
                        .code {{
                            background-color: #99aab5;
                            color: #23272a;
                            font-size: 24px;
                            font-weight: bold;
                            text-align: center;
                            padding: 15px;
                            border-radius: 8px;
                            margin: 20px 0;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #99aab5;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Código de Verificação</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Seu código de verificação é:</p>
                        <div class='code'>{codigoVerificacao}</div>
                        <p>Este código expira em <strong>5 minutos</strong>.</p>
                        <p>Se você não solicitou este código, por favor ignore este email.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de verificação enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de verificação para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public string GerarLinkGoogleCalendar(string titulo, DateTime dataInicio, DateTime dataFim, string descricao, string local)
        {
            dataInicio = dataInicio.ToUniversalTime();
            dataFim = dataFim.ToUniversalTime();

            string dataInicioFormatada = dataInicio.ToString("yyyyMMddTHHmmssZ");
            string dataFimFormatada = dataFim.ToString("yyyyMMddTHHmmssZ");

            return $"https://www.google.com/calendar/render?action=TEMPLATE&text={Uri.EscapeDataString(titulo)}&dates={dataInicioFormatada}/{dataFimFormatada}&details={Uri.EscapeDataString(descricao)}&location={Uri.EscapeDataString(local)}";
        }

        public async Task EnviarEmailFalhaCadastroAsync(string destinatarioEmail, string destinatarioNome, string nomeBarbearia)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de e-mail de falha de cadastro para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Falha no Cadastro - Assistência Necessária";
                var conteudo = $"Olá, {destinatarioNome}!\n\nOcorreu um problema ao concluir o seu cadastro, mas não se preocupe! Nossa equipe está pronta para ajudar você.";

                string htmlContent = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: 'Arial', sans-serif;
                                    background-color: #2c2f33;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    background-color: #23272a;
                                    color: #ffffff;
                                    max-width: 600px;
                                    margin: 20px auto;
                                    border-radius: 10px;
                                    padding: 20px;
                                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                }}
                                h1 {{
                                    font-size: 24px;
                                    color: #e74c3c;
                                    text-align: center;
                                    border-bottom: 2px solid #e74c3c;
                                    padding-bottom: 10px;
                                    margin-bottom: 20px;
                                }}
                                p {{
                                    font-size: 16px;
                                    line-height: 1.6;
                                    color: #ffffff;
                                }}
                                .contact {{
                                    font-weight: bold;
                                    color: #99aab5;
                                }}
                                .footer {{
                                    text-align: center;
                                    margin-top: 20px;
                                    font-size: 12px;
                                    color: #99aab5;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>Assistência no Cadastro</h1>
                                <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                                <p>Ocorreu um problema ao processar o seu cadastro. Nossa equipe está à disposição para resolver isso o mais rápido possível.</p>
                                <p>Por favor, entre em contato conosco pelos canais abaixo para que possamos ajudá-lo:</p>
                                <p class='contact'>WhatsApp: (XX) XXXXX-XXXX<br>
                                E-mail: suporte@cgdreams.com</p>
                                <p>Agradecemos pela sua paciência e estamos ansiosos para atendê-lo.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de falha de cadastro, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail de falha de cadastro, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de falha de cadastro enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de falha de cadastro para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao)
        {
            var assunto = "Redefinição de Senha";
            var conteudo = $"Olá, {destinatarioNome}!\n\nClique no link abaixo para redefinir sua senha:\n{linkRecuperacao}\n\nEste link expira em 1 hora.";

            string htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            background-color: #2c2f33;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            background-color: #23272a;
                            color: #ffffff;
                            max-width: 600px;
                            margin: 20px auto;
                            border-radius: 10px;
                            padding: 20px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{
                            font-size: 24px;
                            color: #e74c3c;
                            text-align: center;
                            border-bottom: 2px solid #e74c3c;
                            padding-bottom: 10px;
                            margin-bottom: 20px;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #ffffff;
                        }}
                        .link {{
                            display: block;
                            width: fit-content;
                            background-color: #99aab5;
                            color: #23272a;
                            font-size: 18px;
                            font-weight: bold;
                            text-align: center;
                            padding: 15px;
                            border-radius: 8px;
                            margin: 20px auto;
                            text-decoration: none;
                        }}
                        .link:hover {{
                            background-color: #7289da;
                            color: #ffffff;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #99aab5;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Redefinição de Senha</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Para redefinir sua senha, clique no link abaixo:</p>
                        <a href='{linkRecuperacao}' class='link'>Redefinir Senha</a>
                        <p>Este link expira em <strong>1 hora</strong>.</p>
                        <p>Se você não solicitou a redefinição de senha, por favor ignore este e-mail.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} BarberShop. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var from = new EmailAddress("barbershopperbrasil@outlook.com", "BarberShop");
            var to = new EmailAddress(destinatarioEmail, destinatarioNome);
            var msg = MailHelper.CreateSingleEmail(from, to, assunto, conteudo, htmlContent);

            var client = new SendGridClient(_sendGridApiKey);
            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de redefinição de senha, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
            }

            await _logService.SaveLogAsync("EmailService", $"E-mail de redefinição de senha enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
        }

        public async Task EnviaEmailAvaliacao(
    int agendamentoId,
    string destinatarioEmail,
    string destinatarioNome,
    string nomeBarbearia,
    string urlBase)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de avaliação para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia);
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = $"{destinatarioNome}, como foi sua última visita?";

                var avaliacaoUrl = $"{urlBase}/Avaliacao/Index?agendamentoId={agendamentoId}";

                string htmlContent = $@"
                <html>
                <head>
                  <meta charset='UTF-8'>
                  <style>
                    body {{
                      font-family: 'Segoe UI', Arial, sans-serif;
                      background-color: #f4f4f4;
                      margin: 0;
                      padding: 0;
                    }}
                    .container {{
                      background-color: #ffffff;
                      max-width: 600px;
                      margin: 20px auto;
                      border-radius: 8px;
                      overflow: hidden;
                      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                    }}
                    .header {{
                      background-color: #e74c3c;
                      color: #fff;
                      padding: 20px;
                      text-align: center;
                      font-size: 20px;
                      font-weight: bold;
                    }}
                    .content {{
                      padding: 20px;
                      color: #333;
                    }}
                    .content p {{
                      line-height: 1.6;
                      font-size: 15px;
                      margin-bottom: 15px;
                    }}
                    .btn {{
                      display: inline-block;
                      background-color: #e74c3c;
                      color: #fff !important;
                      text-decoration: none;
                      font-weight: bold;
                      padding: 12px 20px;
                      border-radius: 5px;
                      font-size: 16px;
                      margin-top: 10px;
                    }}
                    .btn:hover {{
                      background-color: #c0392b;
                    }}
                    .footer {{
                      text-align: center;
                      font-size: 12px;
                      color: #777;
                      padding: 15px;
                      background-color: #f9f9f9;
                    }}
                  </style>
                </head>
                <body>
                  <div class='container'>
                    <div class='header'>
                      {nomeBarbearia}
                    </div>
                    <div class='content'>
                      <p>Olá, <strong>{destinatarioNome}</strong>!</p>
                      <p>Foi um prazer receber você em nossa barbearia. A sua opinião é muito importante para que possamos continuar oferecendo o melhor atendimento.</p>
                      <p>Você poderia nos contar como foi sua experiência?</p>
                      <p style='text-align:center;'>
                        <a href='{avaliacaoUrl}' class='btn'>Avaliar Atendimento</a>
                      </p>
                      <p>Leva menos de 1 minuto e faz toda a diferença para nós. 😉</p>
                    </div>
                    <div class='footer'>
                      &copy; {DateTime.Now.Year} {nomeBarbearia} — Todos os direitos reservados.
                    </div>
                  </div>
                </body>
                </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, string.Empty, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail enviado com sucesso para: {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de avaliação para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }


        public async Task EnviarEmailBoasVindasAsync(string destinatarioEmail, string destinatarioNome, string senha, string tipoUsuario, string nomeBarbearia = null, string urlSlug = null)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de boas-vindas para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Bem-vindo(a) ao Sistema BarberShop!";

                string saudacao = nomeBarbearia != null
                    ? $"Bem-vindo(a) ao sistema da barbearia <strong>{nomeBarbearia}</strong>!"
                    : "Bem-vindo(a) ao nosso sistema!";

                // Recupera o BaseUrl do appSettings
                string baseUrl = _configuration["AppSettings:BaseUrl"];

                // Constrói a URL de acesso
                string accessUrl = $"{baseUrl}/{urlSlug}/Admin";

                string htmlContent = $@"
                                    <html>
                                    <head>
                                        <style>
                                            body {{
                                                font-family: 'Arial', sans-serif;
                                                background-color: #2c2f33;
                                                margin: 0;
                                                padding: 0;
                                            }}
                                            .container {{
                                                background-color: #23272a;
                                                color: #ffffff;
                                                max-width: 600px;
                                                margin: 20px auto;
                                                border-radius: 10px;
                                                padding: 20px;
                                                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                            }}
                                            h1 {{
                                                font-size: 24px;
                                                color: #e74c3c;
                                                text-align: center;
                                                border-bottom: 2px solid #e74c3c;
                                                padding-bottom: 10px;
                                                margin-bottom: 20px;
                                            }}
                                            p {{
                                                font-size: 16px;
                                                line-height: 1.6;
                                                color: #ffffff;
                                            }}
                                            .details {{
                                                background-color: #99aab5;
                                                padding: 15px;
                                                border-radius: 8px;
                                                margin-bottom: 20px;
                                                color: #23272a;
                                            }}
                                            .footer {{
                                                text-align: center;
                                                margin-top: 20px;
                                                font-size: 12px;
                                                color: #99aab5;
                                            }}
                                            .button {{
                                                display: inline-block;
                                                padding: 10px 20px;
                                                font-size: 16px;
                                                color: #ffffff;
                                                background-color: #e74c3c;
                                                text-decoration: none;
                                                border-radius: 5px;
                                                margin-top: 20px;
                                            }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='container'>
                                            <h1>Bem-vindo(a), {destinatarioNome}!</h1>
                                            <p>{saudacao}</p>
                                            <p>Você foi registrado(a) como <strong>{tipoUsuario}</strong>.</p>
                                            <p>Aqui estão suas credenciais de acesso:</p>
                                            <div class='details'>
                                                <p><strong>Login:</strong> {destinatarioEmail}</p>
                                                <p><strong>Senha:</strong> {senha}</p>
                                            </div>
                                            <p>Por favor, altere sua senha após o primeiro acesso para garantir sua segurança.</p>
                                            <p>Para acessar o sistema, clique no botão abaixo:</p>
                                            <p style='text-align: center;'>
                                                <a href='{accessUrl}' class='buttonEmail'>Acesse por aqui</a>
                                            </p>
                                            <p>Se precisar de ajuda, entre em contato com o suporte.</p>
                                            <div class='footer'>
                                                <p>&copy; {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.</p>
                                            </div>
                                        </div>
                                    </body>
                                    </html>";

                var plainTextContent = $@"
                Bem-vindo(a), {destinatarioNome}!

                {saudacao}

                Você foi registrado(a) como {tipoUsuario}.

                Suas credenciais de acesso são:
                Login: {destinatarioEmail}
                Senha: {senha}

                Para acessar o sistema, visite: {accessUrl}

                Por favor, altere sua senha após o primeiro acesso.

                Se precisar de ajuda, entre em contato com o suporte.

                © {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de boas-vindas, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de boas-vindas enviado com sucesso para {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de boas-vindas para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }


        public async Task EnviarEmailCancelamentoAgendamentoAsync(string destinatarioEmail, string destinatarioNome, string nomeBarbearia, DateTime dataHora, string barbeiroNome, string baseUrl)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService", $"Iniciando envio de email de cancelamento para {destinatarioEmail}", "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(destinatarioEmail, destinatarioNome);
                var assunto = "Agendamento Cancelado - Informações Importantes";

                // Gerar URL para reagendamento com base no esquema, domínio e nome da barbearia
                var urlReagendar = $"{baseUrl}/{nomeBarbearia.ToLower().Replace(" ", "")}";

                string htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            background-color: #2c2f33;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            background-color: #23272a;
                            color: #ffffff;
                            max-width: 600px;
                            margin: 20px auto;
                            border-radius: 10px;
                            padding: 20px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{
                            font-size: 24px;
                            color: #e74c3c;
                            text-align: center;
                            border-bottom: 2px solid #e74c3c;
                            padding-bottom: 10px;
                            margin-bottom: 20px;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #ffffff;
                        }}
                        .details {{
                            background-color: #99aab5;
                            padding: 15px;
                            border-radius: 8px;
                            margin-bottom: 20px;
                            color: #23272a;
                        }}
                        .cta {{
                            text-align: center;
                            margin-top: 20px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 12px 25px;
                            font-size: 16px;
                            color: #ffffff;
                            background-color: #e74c3c;
                            text-decoration: none;
                            border-radius: 20px; /* Bordas arredondadas */
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
                            transition: background-color 0.3s ease, box-shadow 0.3s ease;
                        }}
                        .button:hover {{
                            background-color: #c0392b;
                            box-shadow: 0 6px 8px rgba(0, 0, 0, 0.3);
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #99aab5;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Seu Agendamento foi Cancelado</h1>
                        <p>Olá, <strong>{destinatarioNome}</strong>,</p>
                        <p>Infelizmente, seu agendamento com o barbeiro <strong>{barbeiroNome}</strong> na data <strong>{dataHora:dd/MM/yyyy - HH:mm}</strong> foi cancelado.</p>
                        <p>Por favor, entre em contato com a barbearia <strong>{nomeBarbearia}</strong> para mais informações ou para reagendar seu atendimento.</p>
                        <div class='details'>
                            <p>Pedimos desculpas por qualquer inconveniente e agradecemos pela sua compreensão.</p>
                        </div>
                        <div class='cta'>
                            <a href='{urlReagendar}' class='button'>Reagendar Agora</a>
                        </div>
                        <p>Se precisar de ajuda, entre em contato conosco diretamente pelo nosso <a href='mailto:suporte@{nomeBarbearia.ToLower().Replace(" ", "")}.com'>e-mail de suporte</a>.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                var plainTextContent = $@"
                Olá, {destinatarioNome},

                Infelizmente, seu agendamento com o barbeiro {barbeiroNome} na data {dataHora:dd/MM/yyyy - HH:mm} foi cancelado.

                Por favor, entre em contato com a barbearia {nomeBarbearia} para mais informações ou para reagendar seu atendimento.

                Pedimos desculpas por qualquer inconveniente e agradecemos pela sua compreensão.

                Para reagendar, visite: {urlReagendar}

                Se precisar de ajuda, entre em contato conosco pelo e-mail suporte@{nomeBarbearia.ToLower().Replace(" ", "")}.com.

                Atenciosamente,
                {nomeBarbearia}

                © {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService", $"Falha ao enviar o e-mail de cancelamento, status code: {response.StatusCode}", "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService", $"E-mail de cancelamento enviado com sucesso para {destinatarioEmail}", "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService", $"Erro ao enviar e-mail de cancelamento para {destinatarioEmail}: {ex.Message}", "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailBoasVindasBarbeiroAsync(string barbeiroEmail, string barbeiroNome, string nomeBarbearia, string urlSlug,
                                                             byte[] barbeariaLogo, string? senhaProvisoria = null)
        {
            try
            {
                await _logService.SaveLogAsync("EmailService",
                    $"Iniciando envio de boas-vindas para barbeiro {barbeiroEmail}",
                    "INFO", _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(barbeiroEmail, barbeiroNome);
                var assunto = $"Bem-vindo(a) à {nomeBarbearia}!";

                string baseUrl = _configuration["AppSettings:BaseUrl"];
                string accessUrl = $"{baseUrl}/{urlSlug}/Admin";

                string blocoCredenciaisHtml = string.IsNullOrWhiteSpace(senhaProvisoria) ? "" : $@"
                <p style='margin:0 0 10px 0;'>Aqui estão suas credenciais provisórias de acesso:</p>
                <div style='background:#f0f2f4;color:#23272a;padding:12px;border-radius:8px;margin:12px 0;border:1px solid #e6e9ee;'>
                  <p style='margin:0;'><strong>Login:</strong> {barbeiroEmail}</p>
                  <p style='margin:0;'><strong>Senha provisória:</strong> {senhaProvisoria}</p>
                </div>
                <p style='margin:0 0 10px 0;'><em>Por favor, altere sua senha no primeiro acesso.</em></p>";

                string blocoCredenciaisTxt = string.IsNullOrWhiteSpace(senhaProvisoria) ? "" : $@"
                    Credenciais provisórias:
                    Login: {barbeiroEmail}
                    Senha provisória: {senhaProvisoria}
                    (Altere a senha no primeiro acesso.)";

                var htmlContent = $@"
                    <!doctype html>
                    <html>
                      <body style='margin:0;padding:0;background:#f4f6f8;'>
                        <div style=""max-width:600px;margin:24px auto;background:#ffffff;color:#333;border-radius:12px;padding:24px;
                                    font-family:Arial,Helvetica,sans-serif;box-shadow:0 4px 12px rgba(0,0,0,0.08);"">

                          {(barbeariaLogo != null && barbeariaLogo.Length > 0
                ? "<div style='text-align:center;margin-bottom:20px;'><img src=\"cid:barbearia-logo\" alt=\"Logo " + nomeBarbearia + "\" style=\"max-width:180px;height:auto;display:inline-block;border:0;outline:none;text-decoration:none;\" /></div>"
                : "")}

                          <h1 style='font-size:24px;color:#e74c3c;text-align:center;margin:0 0 16px;'>Bem-vindo(a) à família {nomeBarbearia}!</h1>

                          <p style='font-size:16px;line-height:1.6;margin:0 0 12px;text-align:center;'>
                            Olá <strong>{barbeiroNome}</strong>, estamos muito felizes em ter você conosco!<br/>
                            Essa é a sua nova casa para gerenciar seus horários, clientes e serviços com praticidade e estilo.
                          </p>

                      {blocoCredenciaisHtml}

                      <div style='text-align:center;margin:24px 0;'>
                        <a href='{accessUrl}' style='display:inline-block;padding:14px 28px;font-size:16px;color:#fff;
                          background:linear-gradient(90deg,#e74c3c,#c0392b);text-decoration:none;border-radius:30px;font-weight:bold;'>
                          Acessar Painel
                        </a>
                      </div>

                      <p style='font-size:15px;line-height:1.5;margin:0 0 16px;text-align:center;'>
                        Explore, personalize e aproveite todos os recursos que preparamos para facilitar o seu trabalho.
                        <br/>Se precisar de algo, estamos sempre por aqui para ajudar.
                      </p>

                      <div style='text-align:center;margin-top:20px;font-size:12px;color:#999;'>
                        <p style='margin:0;'>&copy; {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.</p>
                      </div>
                    </div>
                  </body>
                </html>";

                var plainTextContent = $@"
                Bem-vindo(a) à família {nomeBarbearia}!

                Olá {barbeiroNome}, estamos muito felizes em ter você conosco.
                Essa é a sua nova casa para gerenciar seus horários, clientes e serviços com praticidade.

                {blocoCredenciaisTxt}

                Acesse seu painel: {accessUrl}

                Explore, personalize e aproveite os recursos que preparamos para facilitar o seu trabalho.
                Se precisar de algo, estamos sempre por aqui para ajudar.

                © {DateTime.Now.Year} {nomeBarbearia}. Todos os direitos reservados.
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);

                // Anexa a logo como inline (CID) — mais compatível do que data URI
                if (barbeariaLogo != null && barbeariaLogo.Length > 0)
                {
                    msg.AddAttachment(new Attachment
                    {
                        Content = Convert.ToBase64String(barbeariaLogo),
                        Type = "image/png",              // ajuste se for "image/jpeg" etc.
                        Filename = "logo.png",
                        Disposition = "inline",
                        ContentId = "barbearia-logo"
                    });
                }

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync("EmailService",
                        $"Falha ao enviar boas-vindas barbeiro, status: {response.StatusCode}",
                        "ERROR", _sendGridApiKey);
                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync("EmailService",
                    $"Boas-vindas enviadas ao barbeiro {barbeiroEmail}",
                    "INFO", _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync("EmailService",
                    $"Erro ao enviar boas-vindas barbeiro {barbeiroEmail}: {ex.Message}",
                    "ERROR", _sendGridApiKey);
                throw;
            }
        }

        public async Task EnviarEmailBoasVindasClienteAsync(string clienteEmail, string clienteNome, string nomeBarbearia, string urlSlug, byte[]? barberiaLogo = null)
        {
            try
            {
                await _logService.SaveLogAsync(
                    "EmailService",
                    $"Iniciando envio de email de boas-vindas (cliente) para {clienteEmail}",
                    "INFO",
                    _sendGridApiKey);

                var client = new SendGridClient(_sendGridApiKey);

                var from = new EmailAddress("barbershopperbrasil@outlook.com", nomeBarbearia ?? "BarberShop System");
                var to = new EmailAddress(clienteEmail, clienteNome);
                var assunto = $"Bem-vindo(a) à {nomeBarbearia}!";

                string baseUrl = _configuration["AppSettings:BaseUrl"] ?? string.Empty;

                string accessUrl = string.IsNullOrWhiteSpace(baseUrl)
                    ? $"/{urlSlug}"
                    : $"{baseUrl.TrimEnd('/')}/{urlSlug?.TrimStart('/')}";

                string htmlContent = $@"
                    <html>
                    <head>
                      <meta charset='utf-8' />
                      <style>
                        body {{
                          font-family: 'Arial', sans-serif;
                          background-color: #2c2f33;
                          margin: 0;
                          padding: 0;
                        }}
                        .container {{
                          background-color: #23272a;
                          color: #ffffff;
                          max-width: 600px;
                          margin: 20px auto;
                          border-radius: 10px;
                          padding: 20px;
                          box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                        }}
                        h1 {{
                          font-size: 24px;
                          color: #e74c3c;
                          text-align: center;
                          border-bottom: 2px solid #e74c3c;
                          padding-bottom: 10px;
                          margin-bottom: 20px;
                        }}
                        p {{
                          font-size: 16px;
                          line-height: 1.6;
                          color: #ffffff;
                          margin: 0 0 12px 0;
                        }}
                        .dica {{
                          background-color: #99aab5;
                          color: #23272a;
                          padding: 14px;
                          border-radius: 8px;
                          margin: 14px 0;
                        }}
                        .button {{
                          display: inline-block;
                          padding: 12px 22px;
                          font-size: 16px;
                          color: #ffffff !important;
                          background-color: #e74c3c;
                          text-decoration: none;
                          border-radius: 20px;
                          margin-top: 12px;
                        }}
                        .footer {{
                          text-align: center;
                          margin-top: 18px;
                          font-size: 12px;
                          color: #99aab5;
                        }}
                      </style>
                    </head>
                    <body>
                      <div class='container'>

                        <!-- LOGO via CID -->
                        <div style='text-align:center;margin-bottom:16px;'>
                          {(barberiaLogo != null && barberiaLogo.Length > 0
                                    ? "<img src=\"cid:barbearia-logo\" alt=\"Logo " + nomeBarbearia + "\" style=\"max-width:200px;height:auto;display:inline-block;border:0;outline:none;text-decoration:none;\" />"
                                    : "")}
                        </div>

                        <h1>Bem-vindo(a), {clienteNome}!</h1>

                        <p>É um prazer ter você com a gente na <strong>{nomeBarbearia}</strong>.</p>
                        <p>Agora ficou ainda mais fácil cuidar do seu visual: faça seus agendamentos online, escolha seu barbeiro favorito e acompanhe seus horários quando quiser.</p>

                        <div class='dica'>
                          <p style='margin:0;'><strong>Dica:</strong> salve o link abaixo para agendar quando precisar:</p>
                          <p style='margin:8px 0 0 0;word-break:break-word;'>
                            <a href='{accessUrl}' style='color:#23272a;text-decoration:underline;'>{accessUrl}</a>
                          </p>
                        </div>

                        <p style='text-align:center;'>
                          <a class='button' href='{accessUrl}'>Agendar agora</a>
                        </p>

                        <p>Se precisar de ajuda, é só responder este e‑mail. Vamos adorar te atender!</p>

                        <div class='footer'>
                          <p style='margin:0;'>&copy; {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.</p>
                        </div>
                      </div>
                    </body>
                    </html>";

                string plainTextContent = $@"
                    Bem-vindo(a), {clienteNome}!

                    É um prazer ter você com a gente na {nomeBarbearia}.
                    Agende online, escolha seu barbeiro favorito e acompanhe seus horários.

                    Acesse: {accessUrl}

                    Se precisar de ajuda, é só responder este e‑mail.

                    © {DateTime.Now.Year} {nomeBarbearia ?? "BarberShop System"}. Todos os direitos reservados.
                    ";

                var msg = MailHelper.CreateSingleEmail(from, to, assunto, plainTextContent, htmlContent);

                if (barberiaLogo != null && barberiaLogo.Length > 0)
                {
                    msg.AddAttachment(new Attachment
                    {
                        Content = Convert.ToBase64String(barberiaLogo),
                        Type = "image/png", // ajuste se sua logo for jpeg/svg
                        Filename = "logo.png",
                        Disposition = "inline",
                        ContentId = "barbearia-logo"
                    });
                }

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    await _logService.SaveLogAsync(
                        "EmailService",
                        $"Falha ao enviar e-mail de boas-vindas (cliente), status: {response.StatusCode}",
                        "ERROR",
                        _sendGridApiKey);

                    throw new Exception($"Falha ao enviar o e-mail, status code: {response.StatusCode}");
                }

                await _logService.SaveLogAsync(
                    "EmailService",
                    $"E-mail de boas-vindas (cliente) enviado com sucesso para {clienteEmail}",
                    "INFO",
                    _sendGridApiKey);
            }
            catch (Exception ex)
            {
                await _logService.SaveLogAsync(
                    "EmailService",
                    $"Erro ao enviar e-mail de boas-vindas (cliente) para {clienteEmail}: {ex.Message}",
                    "ERROR",
                    _sendGridApiKey);
                throw;
            }
        }


    }

}

