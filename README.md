# Horária - Projeto de Alocação de Horários

Aplicação web em ASP.NET Core MVC (.NET 10) para organizar horários acadêmicos, construída a partir do documento de requisitos fornecido.

## Executar no Visual Studio

1. Abra `ProjetoAlocacaoHorarios.slnx` no Visual Studio 2026 ou posterior.
2. Defina `ProjetoAlocacaoHorarios.Web` como projeto de inicialização.
3. Pressione `F5`.

Também pode ser executado pelo terminal:

```powershell
dotnet run --project .\ProjetoAlocacaoHorarios.Web
```

Na primeira execução, o SQLite cria o banco e inclui dados de demonstração.

## Contas de demonstração

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Administrador | admin@horarios.local | Horario@2026 |
| Coordenador | coordenador@horarios.local | Horario@2026 |
| Professor | professor@horarios.local | Horario@2026 |

## Entregas implementadas

- MVC com ASP.NET Core Identity e papéis Administrador, Coordenador e Professor.
- Entity Framework Core com SQLite e modelo normalizado para cursos, turmas, disciplinas, professores, slots, disponibilidade, alocações e grade.
- Gerador heurístico que impede conflitos de professor e turma e respeita a disponibilidade declarada.
- Validação de inviabilidade com mensagens em linguagem acadêmica.
- Painel moderno, visualização da grade e exportação CSV compatível com Excel.

> Para ambiente produtivo, substitua as contas de demonstração e configure persistência segura de chaves de autenticação e envio de e-mail para recuperação de senha.
