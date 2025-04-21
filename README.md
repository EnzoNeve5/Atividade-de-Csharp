# Atividade de C#

## 1. Violação do Princípio da Responsabilidade Única (SRP - Single Responsibility Principle)
### Classe violadora: GerenciadorBiblioteca
### Problema:
Essa classe está fazendo tudo: Gerencia livros, usuários e empréstimos, envia e-mails e SMS, calcula multas e lida com regras de negócios e I/O (como Console.WriteLine)
### Melhoria sugerida:
Separar responsabilidades em classes específicas, por exemplo: ServicoNotificacao, ServicoEmprestimo, RepositorioLivro, RepositorioUsuario, etc.
### Código sugerido:
```
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
```
## 2. Violação do Princípio Aberto/Fechado (OCP - Open/Closed Principle)
### Classe violadora: GerenciadorBiblioteca
### Problema:
Se precisar adicionar um novo meio de notificação (ex: WhatsApp), será necessário modificar a classe e o método RealizarEmprestimo.
### Melhoria sugerida:
Criar uma interface INotificador e implementar para EmailNotificador, SMSNotificador, etc.

O GerenciadorBiblioteca usaria essas abstrações, ficando aberto para extensão, fechado
para modificação.
### Código sugerido:
```
public class EmailService : IEmailService
{
  public void EnviarEmail(string destinatario, string assunto, string mensagem)
  {
    Console.WriteLine($"E-mail enviado para {destinatario}.Assunto: {assunto}");
  }
}
public class SmsService : ISmsService
{
  public void EnviarSMS(string destinatario, string mensagem)
  {
    Console.WriteLine($"SMS enviado para {destinatario}:{mensagem}");
  }
}
```
## 3. Violação do Princípio da Inversão de Dependência (DIP - Dependency Inversion Principle)
### Classe violadora: GerenciadorBiblioteca
### Problema:
A classe depende diretamente de métodos concretos (EnviarEmail, EnviarSMS), Isso
acopla fortemente a lógica de negócios aos detalhes de implementação de notificação.

### Melhoria sugerida:
Depender de abstrações (INotificador) e injeção de dependência para maior flexibilidade.
### Código sugerido:
```
class Program
{
  static void Main()
  {
    var emailService = new EmailService();
    var smsService = new SmsService();
    var biblioteca = new ServicoBiblioteca(emailService,smsService);
    biblioteca.AdicionarLivro("Clean Code", "Robert C.Martin", "978-0132350884");
    biblioteca.AdicionarLivro("Design Patterns", "Erich Gamma", "978-0201633610");
    biblioteca.AdicionarUsuario("João Silva", 1);
    biblioteca.AdicionarUsuario("Maria Oliveira", 2);
    biblioteca.RealizarEmprestimo(1, "978-0132350884", 7);
    double multa = biblioteca.RealizarDevolucao("978-0132350884", 1);
    Console.WriteLine($"Multa por atraso: R$ {multa:F2}");
    Console.ReadLine();
  }
}
```
## 4. Violação do Princípio da Segregação de Interfaces (ISP - Interface Segregation Principle)
Consequência indireta da violação do DIP
### Problema:
Se fosse criada uma interface como INotificador com métodos para e-mail e SMS, uma classe que só envia e-mail seria forçada a implementar EnviarSMS(), mesmo sem precisar.
### Melhoria sugerida:
Separar interfaces por propósito:
```
public interface IEmailService { void EnviarEmail(...); }
public interface ISMSService { void EnviarSMS(...); }
```
### Código sugerido:
```
public interface IEmailService
{
  void EnviarEmail(string destinatario, string assunto, string mensagem);
}
public interface ISmsService
{
  void EnviarSMS(string destinatario, string mensagem);
}
```
## 5. Má prática de Clean Code: nomes genéricos e acoplamento ao Console
### Problemas:
Métodos como AdicionarLivro, AdicionarUsuario, RealizarEmprestimo misturam lógica de negócio com saída no console (Console.WriteLine).

EnviarEmail e EnviarSMS são métodos hardcoded, dificultando testes e reaproveitamento.

Nomes como l, u em variáveis locais são pouco descritivos.
### Melhoria sugerida:
Extraia lógica de exibição para uma camada de interface (UI).

Use nomes significativos (livro, usuario, etc.).

Evite Console.WriteLine dentro da lógica de negócio.

Use eventos, logs ou notificadores.
### Código sugerido:
```
public class Livro
{
  public string Titulo { get; set; }
  public string Autor { get; set; }
  public string ISBN { get; set; }
  public bool Disponivel { get; set; } = true;
}
public class Usuario
{
  public string Nome { get; set; }
  public int ID { get; set; }
}
public class Emprestimo
{
  public Livro Livro { get; set; }
  public Usuario Usuario { get; set; }

  public DateTime DataEmprestimo { get; set; }
  public DateTime DataDevolucaoPrevista { get; set; }
  public DateTime? DataDevolucaoEfetiva { get; set; }
}
```
