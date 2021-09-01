using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolsynAnalysis
{
    public static class Symbols
    {
        public static void ReviewSymbolTable(Compilation compilation)
        {
            foreach (var member in compilation.Assembly.GlobalNamespace.GetMembers()
            .Where(member => member.CanBeReferencedByName))
            {
                Console.WriteLine(member.Name);
                foreach (var item in member.GetTypeMembers()
                .Where(item => item.CanBeReferencedByName))
                {
                    Console.WriteLine("\t{0}:{1}", item.TypeKind, item.Name);
                    foreach (var innerItem in item.GetMembers()
                        .Where(innerItem => innerItem.CanBeReferencedByName))
                    {
                        Console.WriteLine("\t\t{0}:{1}", innerItem.Kind, innerItem.Name);
                    }
                }
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseClass(SemanticModel model, BaseTypeDeclarationSyntax type)
        {
            var classSymbol = model.GetDeclaredSymbol(type) as INamedTypeSymbol;
            var returnvalue = new List<INamedTypeSymbol>();
            while (classSymbol?.BaseType != null)
            {
                returnvalue.Add(classSymbol.ContainingType);
                if (classSymbol.Interfaces != null)
                    returnvalue.AddRange(classSymbol.Interfaces);
                classSymbol = classSymbol.BaseType;
            }
            return returnvalue;
        }

        public static IEnumerable<BaseTypeDeclarationSyntax> FindClassDerivedOrImplementedByType(Compilation compilation,INamedTypeSymbol target)
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                foreach (var type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    var baseClass = GetBaseClass(semanticModel,type);
                    if (baseClass != null)
                        if (baseClass.Contains(target))
                        {
                            yield return type;
                        }
                }
            }
        }

        public static SyntaxTree ReplaceClassName(string className)
        {
            var code = @"
public class Class1
{
}
";
            var synTree = CSharpSyntaxTree.ParseText(code);
            var identifierToken = synTree.GetRoot().DescendantTokens()
 .First(t => t.IsKind(SyntaxKind.IdentifierToken)
 && t.Parent.Kind() == SyntaxKind.ClassDeclaration);
            var newIdentifier = SyntaxFactory.Identifier(className);
            return SyntaxFactory.SyntaxTree(synTree.GetRoot()
            .ReplaceToken(identifierToken, newIdentifier));
        }


        #region AddProperty
        public static ClassDeclarationSyntax AddProperty(this ClassDeclarationSyntax currentClass, string name, INamedTypeSymbol type)
        {
            if (currentClass.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Any(p => p.Identifier.Text == name))
            {
                // class already has the specified property
                return currentClass;
            }
            var typeSentax = SyntaxFactory.ParseTypeName(type.Name);
            var newProperty = SyntaxFactory.PropertyDeclaration(typeSentax, name)
            .WithModifiers(
            SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(
            SyntaxFactory.AccessorList(
            SyntaxFactory.List(
            new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            })));
            return currentClass.AddMembers(newProperty);
        }

        public static void CreateMetadataByCurrent()
        {
            var refference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("internal").WithReferences(refference);
            var intType = compilation.GetTypeByMetadataName("System.Int32");
            var stringType = compilation.GetTypeByMetadataName("System.String");
            var dateTimeType = compilation.GetTypeByMetadataName("System.DateTime");
            var emptyClassTree = ReplaceClassName("GreetingBusinessRule");
            var emptyClass = emptyClassTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (emptyClass == null)
                return;
            emptyClass = emptyClass.AddProperty("TAge", intType)
                .AddProperty("TFirstName", stringType)
                .AddProperty("TLastName", stringType)
                .AddProperty("TDateOfBirth", dateTimeType)
                .NormalizeWhitespace();
            Console.WriteLine(emptyClass.ToString());
        } 
        #endregion
        #region CreateEnum Attribute Member
        public static void CreateEnum()
        {
            var code = @"
[EnumDescription("" "")]
public enum EnumName
{
[MemberDescription ("" "")]
Name = Value;
}";
            foreach (var data in EnumTypeItem.getList())
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var newSyntaxTree = syntaxTree.GetRoot();
                if (!string.IsNullOrEmpty(data.LongDescription))
                {
                    var literal = newSyntaxTree.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault();
                    var newLiteral = SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(data.LongDescription));
                    newSyntaxTree = newSyntaxTree.ReplaceNode(literal, newLiteral);
                }
                else
                {
                    var attribute = newSyntaxTree.DescendantNodes()
                    .OfType<AttributeSyntax>().FirstOrDefault();
                    if (attribute != null)
                        newSyntaxTree = newSyntaxTree.RemoveNode
                        (attribute, SyntaxRemoveOptions.KeepNoTrivia);
                }

                var identifierToken = newSyntaxTree.DescendantTokens()
                        .First(t => t.IsKind(SyntaxKind.IdentifierToken)
                        && t.Parent.Kind() == SyntaxKind.EnumDeclaration);
                var newIdentifier = SyntaxFactory.Identifier(data.ShortDescription.Replace(" ", ""));
                newSyntaxTree = SyntaxFactory.SyntaxTree(newSyntaxTree.ReplaceToken(identifierToken, newIdentifier)).GetRoot();
                List<EnumMemberDeclarationSyntax> li = new List<EnumMemberDeclarationSyntax>();
                foreach (var item in data.details)
                {
                    var memberDeclaration = newSyntaxTree.DescendantNodes()
                            .OfType<EnumMemberDeclarationSyntax>()
                            .FirstOrDefault();
                    memberDeclaration = memberDeclaration.WithIdentifier(SyntaxFactory.Identifier(
                        item.ShortDescription.Replace(" ", "")));

                    memberDeclaration = memberDeclaration.WithEqualsValue(SyntaxFactory
                            .EqualsValueClause(SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(item.LoanCodeId))));

                    var attributeDeclaration = memberDeclaration.DescendantNodes().OfType<AttributeListSyntax>().FirstOrDefault();
                    if (!string.IsNullOrEmpty(item.LongDescription))
                    {
                        var description =
                        attributeDeclaration.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault();
                        var newDescription = SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item.LongDescription));
                        var newAttribute = attributeDeclaration.ReplaceNode(description, newDescription);
                        memberDeclaration = memberDeclaration.ReplaceNode(attributeDeclaration, newAttribute);
                        li.Add(memberDeclaration);
                    }
                    else
                    {
                        memberDeclaration = memberDeclaration.RemoveNode
                        (attributeDeclaration,
                        SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }

                var firstMember = newSyntaxTree.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().FirstOrDefault();
                newSyntaxTree = newSyntaxTree.RemoveNode(firstMember, SyntaxRemoveOptions.KeepNoTrivia);
                if (li.Count != 0)
                {
                    var declaraNodes = newSyntaxTree.ChildNodes().OfType<EnumDeclarationSyntax>().FirstOrDefault();
                    EnumDeclarationSyntax newDeclarationSyntax = declaraNodes.AddMembers(li.ToArray());
                    newSyntaxTree = newSyntaxTree.ReplaceNode(declaraNodes, newDeclarationSyntax);
                }
                Console.WriteLine(newSyntaxTree.ToFullString());
            }
        }

        #endregion
        #region CreateClass
        public static void CreateRules()
        {
            var code = @"
public class GreetingRules{
public string Rule(IGreetingProfile data){ }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code).GetRoot();
            var classDeclaration = syntaxTree.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var originalDeclaration = classDeclaration;

            foreach (var rule in GreetingRuleDetail.getList())
            {
                var greetingRule = syntaxTree.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                //GlobalStatementSyntax greetingRule = methodSyntaxTree.DescendantNodes().OfType<GlobalStatementSyntax>().FirstOrDefault();
                var identifierToken = greetingRule.DescendantTokens()
               .First(t => t.IsKind(SyntaxKind.IdentifierToken)
               && t.Parent.Kind() == SyntaxKind.MethodDeclaration);
                var newIdentifier = SyntaxFactory.Identifier("Rule" + rule.GreetingRuleId);
                greetingRule = greetingRule.ReplaceToken(identifierToken, newIdentifier);
                if (rule.HourMin.HasValue)
                {
                    var newBlock = ProcessHourCondition(rule.HourMin.Value, greetingRule.Body, SyntaxKind.LessThanExpression);
                    greetingRule = greetingRule.WithBody(newBlock);
                }
                if (rule.HourMax.HasValue)
                {
                    var newBlock = ProcessHourCondition(rule.HourMax.Value, greetingRule.Body, SyntaxKind.GreaterThanExpression);
                    greetingRule = greetingRule.WithBody(newBlock);
                }
                if (rule.Gender.HasValue)
                {
                    var newBlock = ProcessEqualityComparison("Gender", rule.Gender.Value, greetingRule.Body);
                    greetingRule = greetingRule.WithBody(newBlock);
                }
                if (rule.MaritalStatus.HasValue)
                {
                    var newBlock = ProcessEqualityComparison("MaritalStatus", rule.MaritalStatus.Value, greetingRule.Body);
                    greetingRule = greetingRule.WithBody(newBlock);
                }
                var currentBlock = AddRuleReturnValue(greetingRule.Body, rule);
                greetingRule = greetingRule.WithBody(currentBlock);
                classDeclaration = classDeclaration.AddMembers(greetingRule);
            }
            var oldgreetingRule = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            classDeclaration = classDeclaration.RemoveNode(oldgreetingRule, SyntaxRemoveOptions.KeepDirectives);
            syntaxTree = syntaxTree.ReplaceNode(originalDeclaration, classDeclaration);
            Console.WriteLine(syntaxTree.ToFullString());
        }
        private static ReturnStatementSyntax ReturnNull()
        {
            return SyntaxFactory.ReturnStatement
            (SyntaxFactory.LiteralExpression(
            SyntaxKind.NullLiteralExpression));
        }
        private static BlockSyntax AddRuleReturnValue(BlockSyntax currentBlock, GreetingRuleDetail rule)
        {
            var ruleGreeting = SyntaxFactory.LiteralExpression
            (SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(rule.Greeting));
            var lastName = SyntaxFactory.MemberAccessExpression
            (SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("data"),
            SyntaxFactory.IdentifierName("LastName"));
            var assignment = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, ruleGreeting, lastName);
            var returnStatement = SyntaxFactory.ReturnStatement(assignment);
            return currentBlock.AddStatements(new StatementSyntax[] { returnStatement });
        }
        private static BlockSyntax ProcessHourCondition(int hourValue, BlockSyntax currentBlock, SyntaxKind comparisonType)
        {
            var hourExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("data"),
            SyntaxFactory.IdentifierName("Hour"));
            var condition = SyntaxFactory.BinaryExpression(comparisonType,
            hourExpression,
            SyntaxFactory.LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(hourValue)));
            var newConditional = SyntaxFactory.IfStatement(
            condition, ReturnNull());
            return currentBlock.AddStatements(new StatementSyntax[] { newConditional });
        }
        private static BlockSyntax ProcessEqualityComparison(string whichEquality, int value, BlockSyntax currentBlock)
        {
            var genderReference = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("data"),
            SyntaxFactory.IdentifierName(whichEquality));
            var condition = SyntaxFactory.BinaryExpression
            (SyntaxKind.NotEqualsExpression,
            genderReference, SyntaxFactory.LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(value)));
            var newConditional = SyntaxFactory.IfStatement(condition, ReturnNull());
            return currentBlock.AddStatements(new StatementSyntax[] { newConditional });
        }
        #endregion
        #region GerateRules
        public static void GerateRules()
        {
            var code = @"
public class UnderwritingRules{
public bool Rule(ILoanCodes data)
{
var target = new []{};
}
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code).GetRoot();
            var classDeclaration = syntaxTree.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var originalDeclaration = classDeclaration;
            foreach (var rule in LoanCodes.getlist())
            {
                var methodSyntaxTree = CSharpSyntaxTree.ParseText(code).GetRoot();
                var underwritingRule = methodSyntaxTree.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                underwritingRule = RenameRule(rule.RuleName, underwritingRule);
                underwritingRule = underwritingRule.WithLeadingTrivia(new SyntaxTrivia[] { SyntaxFactory.Comment("//" + rule.ShortDescription) });
                underwritingRule = underwritingRule.WithBody(ProcessLoanCodes(rule, underwritingRule.Body));
                var currentBlock = underwritingRule.Body;
                currentBlock = currentBlock.AddStatements(new StatementSyntax[] { ReturnTrue() });
                underwritingRule = underwritingRule.WithBody(currentBlock);
                classDeclaration = classDeclaration.AddMembers(underwritingRule);
            }
            Console.WriteLine(classDeclaration.NormalizeWhitespace());
        }

        public static MethodDeclarationSyntax RenameRule(string name, MethodDeclarationSyntax underwrittingRule)
        {
            var identifierToken = underwrittingRule.DescendantTokens()
                    .First(t => t.IsKind(SyntaxKind.IdentifierToken) && t.Parent.Kind() == SyntaxKind.MethodDeclaration);
            var newIdentifier = SyntaxFactory.Identifier("Rule" + name);
            underwrittingRule = underwrittingRule.ReplaceToken(identifierToken, newIdentifier);
            return underwrittingRule;
        }
        private static ReturnStatementSyntax ReturnFalse()
        {
            return SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
        }

        private static ReturnStatementSyntax ReturnTrue()
        {
            return SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
        }

        private static BlockSyntax ProcessLoanCodes(LoanCodes rule, BlockSyntax underwritingRule)
        {
            var loaCodeTypes = from d in rule.details
                               group d by new { d.LoanCodeTypeId, d.IsRange }
                              into loanCodes
                               select loanCodes;
            foreach (var loanCodeType in loaCodeTypes)
            {
                var loanCodeTypeId = loanCodeType.Key.LoanCodeTypeId;
                if (!loanCodeType.Key.IsRange)
                {
                    var initialzationExpressions = new List<LiteralExpressionSyntax>();
                    foreach (var detail in loanCodeType)
                    {
                        initialzationExpressions.Add(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(detail.LoanCodeId)));
                        underwritingRule = ProcessLoanCodeCondition(loanCodeTypeId, underwritingRule, initialzationExpressions);
                    }
                }
                else
                {
                    foreach (var detail in loanCodeType)
                    {
                        if (detail.Max.HasValue)
                        {
                            underwritingRule = ProcessLoanCodeRangeCondition(loanCodeTypeId, underwritingRule, SyntaxKind.GreaterThanExpression, detail.Max.Value);
                        }
                        if (detail.Min.HasValue)
                        {
                            underwritingRule = ProcessLoanCodeRangeCondition(loanCodeTypeId, underwritingRule, SyntaxKind.LessThanExpression, detail.Min.Value);
                        }
                    }
                }
            }
            return underwritingRule;
        }

        private static BlockSyntax ProcessLoanCodeRangeCondition(int loanCodeTypeId, BlockSyntax underwritingRule, SyntaxKind comparisonType, decimal codeValue)
        {
            var codeExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("data"),
            SyntaxFactory.IdentifierName("Code" + loanCodeTypeId));
            var condition = SyntaxFactory.BinaryExpression(comparisonType,
            codeExpression, SyntaxFactory.LiteralExpression
            (SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(codeValue)));
            var newConditional = SyntaxFactory.IfStatement(condition,
            ReturnFalse());
            return underwritingRule.AddStatements(new StatementSyntax[] { newConditional });
        }

        private static BlockSyntax ProcessLoanCodeCondition(int loanCode, BlockSyntax currentBlock, List<LiteralExpressionSyntax> initializationExpressions)
        {
            var assignmentStatement = ReinitializeTargetArray(currentBlock, initializationExpressions);
            currentBlock = currentBlock.AddStatements(new StatementSyntax[] { assignmentStatement });
            var codeExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("data"),
            SyntaxFactory.IdentifierName("Code" + loanCode));
            var target = SyntaxFactory.MemberAccessExpression
            (SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("target"),
            SyntaxFactory.IdentifierName("Contains"));
            var argument = SyntaxFactory.Argument(codeExpression);
            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });
            var contains = SyntaxFactory.InvocationExpression(target,
            SyntaxFactory.ArgumentList(argumentList));
            var notContains = SyntaxFactory.BinaryExpression
            (SyntaxKind.NotEqualsExpression, contains, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));// target.contain(data.Code) == true
            var newConditional = SyntaxFactory.IfStatement(notContains, ReturnFalse());
            return currentBlock.AddStatements(new StatementSyntax[] { newConditional });
        }

        private static ExpressionStatementSyntax ReinitializeTargetArray(BlockSyntax currentBlock, List<LiteralExpressionSyntax> initializationExpressions)
        {
            var declarator = currentBlock.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var init = declarator.Initializer;
            var initializationExpression = currentBlock.DescendantNodes().OfType<ImplicitArrayCreationExpressionSyntax>().FirstOrDefault();
            initializationExpression = initializationExpression.AddInitializerExpressions(initializationExpressions.ToArray());
            var variableIdentifier = SyntaxFactory.IdentifierName("target");
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, initializationExpression); 
            var assignmentStatement = SyntaxFactory.ExpressionStatement(assignment);
            return assignmentStatement;
        }

        #endregion

        #region CreateClassAndMethodWithSyntax
        public static void CreateClassAndMethodWithSyntax()
        {
            var consolewriteline = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Console"),
                SyntaxFactory.IdentifierName("WriteLine"));
            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(@"""Goodbye everyone!""", "Goodbye everyone!")));
            var arguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { argument }));
            var consolewritelineStatement = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(consolewriteline, arguments));
            var voidType = SyntaxFactory.ParseTypeName("void");
            var method = SyntaxFactory.MethodDeclaration(voidType, "MyMethod").WithBody(SyntaxFactory.Block(consolewritelineStatement)).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var intType = SyntaxFactory.ParseTypeName("int");
            var getterBody = SyntaxFactory.ReturnStatement(SyntaxFactory.DefaultExpression(intType));
            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(getterBody));
            var property = SyntaxFactory.PropertyDeclaration(intType, "YourName").WithAccessorList(
                SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(getter))).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var classs = SyntaxFactory.ClassDeclaration("MyClass").WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] { method, property })).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
            Console.WriteLine(classs.ToFullString());
        }

        // WithTrailingTrivia(SyntaxFactory.LineFeed)：换行
        //.WithTrailingTrivia(SyntaxFactory.Space)：加空格
        public static void CreateClassAndMethodWithSyntax2()
        {
            var consolewriteline = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Console"),
                SyntaxFactory.IdentifierName("WriteLine"));
            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(@"""Goodbye everyone!""", "Goodbye everyone!")));
            var arguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { argument }));
            var consolewritelineStatement = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(consolewriteline, arguments)).WithTrailingTrivia(SyntaxFactory.Space);
            var voidType = SyntaxFactory.ParseTypeName("void").WithTrailingTrivia(SyntaxFactory.Space);
            var method = SyntaxFactory.MethodDeclaration(voidType, "MyMethod").WithBody(SyntaxFactory.Block(consolewritelineStatement)).WithTrailingTrivia(SyntaxFactory.Space).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space))).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var get = SyntaxFactory.IdentifierName("_yourName").WithTrailingTrivia(SyntaxFactory.Space);
            var value = SyntaxFactory.IdentifierName("value").WithTrailingTrivia(SyntaxFactory.Space);
            var getter = SyntaxFactory.ReturnStatement(get).WithTrailingTrivia(SyntaxFactory.Space);
            var setter = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, get, value)).WithTrailingTrivia(SyntaxFactory.Space);
            var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string").WithTrailingTrivia(SyntaxFactory.Space), "YourName").WithTrailingTrivia(SyntaxFactory.Space)
                .WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space)))
                   .AddAccessorListAccessors(
                       SyntaxFactory.AccessorDeclaration(
                           SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.List(new[] { getter }))),
                       SyntaxFactory.AccessorDeclaration(
                           SyntaxKind.SetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.List(new[] { setter })))
                   ).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var newLiteral = SyntaxFactory.LiteralExpression
                   (SyntaxKind.StringLiteralExpression,
                   SyntaxFactory.Literal("Hello Word!"));
            //var defaultvalue = SyntaxFactory.EqualsValueClause(newLiteral);
            var defaultvalue = SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName("string")));
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseName("String").WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_yourName") ).WithInitializer(defaultvalue) }))//.WithTrailingTrivia(SyntaxFactory.Space),
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var classs = SyntaxFactory.ClassDeclaration("MyClass").WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] { method, fieldDeclaration, property })).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space)));
            Console.WriteLine(classs.ToFullString());
        }

        public static void CreateClassAndMethodWithSyntax3()
        {
            var consolewriteline = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Console"),
                SyntaxFactory.IdentifierName("WriteLine"));
            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(@"""Goodbye everyone!""", "Goodbye everyone!")));
            var arguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { argument }));
            var consolewritelineStatement = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(consolewriteline, arguments)).WithTrailingTrivia(SyntaxFactory.Space);
            var voidType = SyntaxFactory.ParseTypeName("void").WithTrailingTrivia(SyntaxFactory.Space);
            var method = SyntaxFactory.MethodDeclaration(voidType, "MyMethod").WithBody(SyntaxFactory.Block(consolewritelineStatement)).WithTrailingTrivia(SyntaxFactory.Space).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space))).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var get = SyntaxFactory.IdentifierName("_yourName").WithTrailingTrivia(SyntaxFactory.Space);
            var value = SyntaxFactory.IdentifierName("value").WithTrailingTrivia(SyntaxFactory.Space);
            var getter = SyntaxFactory.ReturnStatement(get).WithTrailingTrivia(SyntaxFactory.Space);
            var setter = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, get, value)).WithTrailingTrivia(SyntaxFactory.Space);

            var intType = SyntaxFactory.ParseTypeName("int").WithTrailingTrivia(SyntaxFactory.Space);
            var identity = SyntaxFactory.IdentifierName("LoanCodeID");
            var k = SyntaxFactory.IdentifierName("k");
            var condition = SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, identity, k);
            var gette = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("_yourName").WithTrailingTrivia(SyntaxFactory.Space)).WithTrailingTrivia(SyntaxFactory.Space);
            var addExpression = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, identity, k).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var delExpression = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, identity, k).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var del = SyntaxFactory.ReturnStatement(delExpression).WithTrailingTrivia(SyntaxFactory.Space);
            var elseclase = SyntaxFactory.ElseClause(del).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var newcondition = SyntaxFactory.IfStatement(condition,
                SyntaxFactory.ReturnStatement(addExpression), elseclase).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var method2 = SyntaxFactory.MethodDeclaration(intType, "myMethod2").WithBody(SyntaxFactory.Block(newcondition.WithTrailingTrivia(SyntaxFactory.Space)))
                .WithTrailingTrivia(SyntaxFactory.LineFeed)
                .WithParameterList(SyntaxFactory.ParameterList(
SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Parameter(k.Identifier).WithType(intType).WithoutLeadingTrivia())));

            var defaultvalue2 = SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(intType));
            var fieldDeclaration2 = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(intType.WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("LoanCodeID") ).WithInitializer(defaultvalue2) }))//.WithTrailingTrivia(SyntaxFactory.Space),
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)).WithTrailingTrivia(SyntaxFactory.LineFeed);


            var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string").WithTrailingTrivia(SyntaxFactory.Space), "YourName").WithTrailingTrivia(SyntaxFactory.Space)
                .WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space)))
                   .AddAccessorListAccessors(
                       SyntaxFactory.AccessorDeclaration(
                           SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.List(new[] { getter }))),
                       SyntaxFactory.AccessorDeclaration(
                           SyntaxKind.SetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.List(new[] { setter })))
                   ).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var newLiteral = SyntaxFactory.LiteralExpression
                   (SyntaxKind.StringLiteralExpression,
                   SyntaxFactory.Literal("Hello Word!"));
            //var defaultvalue = SyntaxFactory.EqualsValueClause(newLiteral);
            var defaultvalue = SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName("string")));
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseName("String").WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_yourName") ).WithInitializer(defaultvalue) }))//.WithTrailingTrivia(SyntaxFactory.Space),
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)).WithTrailingTrivia(SyntaxFactory.LineFeed);
            var classs = SyntaxFactory.ClassDeclaration("MyClass").WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] { method, fieldDeclaration2, method2, fieldDeclaration, property })).WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space)));
            Console.WriteLine(classs.ToFullString());
        }
        #endregion

        #region Walker Visit
        public static void WorkerVisit()
        {
            string code = @"
        public  class MyClass{
        private int LoanCodeID = default(int);
        int myMethod2(int k)
        {
            if (LoanCodeID > k)
                return LoanCodeID + k;
            else return LoanCodeID - k;
        }
        private String _yourName = default(string);
        public string YourName { get { return _yourName; } set { _yourName = value; } }
    }
";
            var tree = CSharpSyntaxTree.ParseText(code);
            var walker = new Walker();
            walker.Visit(tree.GetRoot());
        }
        #endregion

    }
}

