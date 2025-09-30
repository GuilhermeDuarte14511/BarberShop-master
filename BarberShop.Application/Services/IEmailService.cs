﻿namespace BarberShop.Application.Services
{
    public interface IEmailService
    {
        Task EnviarEmailAgendamentoAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string assunto,
            string conteudo,
            string barbeiroNome,
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            decimal total,
            string formaPagamento,
            string nomeBarbearia,
            string googleCalendarLink = null);

        Task EnviarEmailNotificacaoBarbeiroAsync(
            string barbeiroEmail,
            string barbeiroNome,
            string clienteNome,
            List<string> servicos,
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            decimal total,
            string formaPagamento,
            string nomeBarbearia);

        Task EnviarEmailCodigoVerificacaoAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string codigoVerificacao,
            string nomeBarbearia);

        string GerarLinkGoogleCalendar(
            string titulo,
            DateTime dataInicio,
            DateTime dataFim,
            string descricao,
            string local);

        Task EnviarEmailFalhaCadastroAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string nomeBarbearia);

        Task EnviarEmailRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao);
        Task EnviaEmailAvaliacao(int agendamentoId, string destinatarioEmail, string destinatarioNome, string nomeBarbearia, string urlBase);
        Task EnviarEmailBoasVindasAsync(string destinatarioEmail, string destinatarioNome, string senha, string tipoUsuario, string nomeBarbearia = null, string urlSlug = null);
        Task EnviarEmailCancelamentoAgendamentoAsync( string destinatarioEmail, string destinatarioNome, string nomeBarbearia, DateTime dataHora, string barbeiroNome, string baseUrl);
        Task EnviarEmailBoasVindasBarbeiroAsync(string barbeiroEmail,string barbeiroNome,string nomeBarbearia,string urlSlug, byte[] barberiaLogo, string? senhaProvisoria = null);
        Task EnviarEmailBoasVindasClienteAsync(
            string clienteEmail,
            string clienteNome,
            string nomeBarbearia,
            string urlSlug,
            byte[]? barberiaLogo = null
        );
    }

}
