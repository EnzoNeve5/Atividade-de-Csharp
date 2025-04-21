using System;
using System.Collections.Generic;

public class ServicoBiblioteca
{
    private List<Livro> livros = new();
    private List<Usuario> usuarios = new();
    private List<Emprestimo> emprestimos = new();

    private readonly IEmailService emailService;
    private readonly ISmsService smsService;

    public ServicoBiblioteca(IEmailService emailService, ISmsService smsService)
    {
        this.emailService = emailService;
        this.smsService = smsService;
    }

    public void AdicionarLivro(string titulo, string autor, string isbn)
    {
        var livro = new Livro { Titulo = titulo, Autor = autor, ISBN = isbn };
        livros.Add(livro);
    }

    public void AdicionarUsuario(string nome, int id)
    {
        var usuario = new Usuario { Nome = nome, ID = id };
        usuarios.Add(usuario);

        emailService.EnviarEmail(usuario.Nome, "Bem-vindo à Biblioteca", "Você foi cadastrado em nosso sistema!");
    }

    public bool RealizarEmprestimo(int usuarioId, string isbn, int diasEmprestimo)
    {
        var livro = livros.Find(l => l.ISBN == isbn && l.Disponivel);
        var usuario = usuarios.Find(u => u.ID == usuarioId);

        if (livro == null || usuario == null) return false;

        livro.Disponivel = false;

        var emprestimo = new Emprestimo
        {
            Livro = livro,
            Usuario = usuario,
            DataEmprestimo = DateTime.Now,
            DataDevolucaoPrevista = DateTime.Now.AddDays(diasEmprestimo)
        };

        emprestimos.Add(emprestimo);

        emailService.EnviarEmail(usuario.Nome, "Empréstimo Realizado", $"Você pegou emprestado o livro: {livro.Titulo}");
        smsService.EnviarSMS(usuario.Nome, $"Empréstimo do livro: {livro.Titulo}");

        return true;
    }

    public double RealizarDevolucao(string isbn, int usuarioId)
    {
        var emprestimo = emprestimos.Find(e =>
            e.Livro.ISBN == isbn &&
            e.Usuario.ID == usuarioId &&
            e.DataDevolucaoEfetiva == null);

        if (emprestimo == null) return -1;

        emprestimo.DataDevolucaoEfetiva = DateTime.Now;
        emprestimo.Livro.Disponivel = true;

        double multa = 0;
        if (DateTime.Now > emprestimo.DataDevolucaoPrevista)
        {
            multa = (DateTime.Now - emprestimo.DataDevolucaoPrevista).Days * 1.0;
            emailService.EnviarEmail(emprestimo.Usuario.Nome, "Multa por Atraso", $"Você tem uma multa de R$ {multa:F2}");
        }

        return multa;
    }

    public List<Livro> BuscarTodosLivros() => livros;
    public List<Usuario> BuscarTodosUsuarios() => usuarios;
    public List<Emprestimo> BuscarTodosEmprestimos() => emprestimos;
}


// Método para enviar e-mail
public class EmailService : IEmailService
{
    // Simulação de envio de e-mail
    public void EnviarEmail(string destinatario, string assunto, string mensagem)
    {
        Console.WriteLine($"E-mail enviado para {destinatario}. Assunto: {assunto}");
    }
}
    // Método para enviar SMS
public class SmsService : ISmsService
{
    // Simulação de envio de SMS
    public void EnviarSMS(string destinatario, string mensagem)
    {
        Console.WriteLine($"SMS enviado para {destinatario}: {mensagem}");
    }
}

public interface IEmailService
{
    void EnviarEmail(string destinatario, string assunto, string mensagem);
}

public interface ISmsService
{
    void EnviarSMS(string destinatario, string mensagem);
}

// Classe de Livro
public class Livro
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string ISBN { get; set; }
    public bool Disponivel { get; set; } = true;
}
// Classe de Usuário
public class Usuario
{
    public string Nome { get; set; }
    public int ID { get; set; }
}
// Classe de Empréstimo
public class Emprestimo
{
    public Livro Livro { get; set; }
    public Usuario Usuario { get; set; }
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataDevolucaoPrevista { get; set; }
    public DateTime? DataDevolucaoEfetiva { get; set; }
}


// Classe Program para testar
class Program
{
    static void Main()
    {
        var emailService = new EmailService();
        var smsService = new SmsService();
        var biblioteca = new ServicoBiblioteca(emailService, smsService);
  
        biblioteca.AdicionarLivro("Clean Code", "Robert C. Martin", "978-0132350884");
        biblioteca.AdicionarLivro("Design Patterns", "Erich Gamma", "978-0201633610");

        biblioteca.AdicionarUsuario("João Silva", 1);
        biblioteca.AdicionarUsuario("Maria Oliveira", 2);
     
        biblioteca.RealizarEmprestimo(1, "978-0132350884", 7);
            
        double multa = biblioteca.RealizarDevolucao("978-0132350884", 1);
        Console.WriteLine($"Multa por atraso: R$ {multa:F2}");
            
        Console.ReadLine();
    }
}