using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using R5T.T0134;

using Instances = R5T.X0016.Instances;


namespace System
{
    public static partial class MemberDeclarationSyntaxExtensions
    {
        public static AddNodeResult<T, AttributeListSyntax> AcquireAttributeList<T>(this T member)
            where T : MemberDeclarationSyntax
        {
            var hasAttributeList = member.HasAttributeList();
            if(hasAttributeList)
            {
                member.AnnotateNode(
                    hasAttributeList.Result,
                    out var attributeListAnnotation);

                var output = AddNodeResult.From(member, attributeListAnnotation);
                return output;
            }
            else
            {
                var output = member.AddAttributeList();
                return output;
            }
        }

        public static AddNodeResult<T, AttributeSyntax> AddAttribute<T>(this T member,
            AttributeSyntax attribute)
            where T : MemberDeclarationSyntax
        {
            ISyntaxNodeAnnotation<AttributeListSyntax> attributeListAnnotation;
            (member, attributeListAnnotation) = member.AcquireAttributeList();

            var attributeAnnotation = SyntaxNodeAnnotation.Initialize<AttributeSyntax>();
            member = attributeListAnnotation.Modify(
                member,
                attributeList =>
                {
                    var annotatedAttribute = attribute.Annotate(out attributeAnnotation);

                    attributeList = attributeList.AddAttributes(annotatedAttribute);

                    return attributeList;
                });

            // Ensure spacing relative to next token.
            var attributeList = attributeListAnnotation.GetNode(member);

            var lastToken = attributeList.GetLastToken();
            var nextToken = lastToken.GetNextToken();

            var lastModifierHasAnyTrailingSeparatingTrivia = lastToken.HasAnyTrailingSeparatingTrivia(nextToken);
            if (!lastModifierHasAnyTrailingSeparatingTrivia)
            {
                // Ok to replace, as the next token *will* be within the member for valid syntax.
                member = member.ReplaceToken_Better(
                    nextToken,
                    nextToken.PrependNewLine());
            }

            var output = AddNodeResult.From(
                member,
                attributeAnnotation);

            return output;
        }

        public static AddNodeResult<T, AttributeListSyntax> AddAttributeList<T>(this T member,
            AttributeListSyntax attributeList)
            where T : MemberDeclarationSyntax
        {
            var output = member.AddNode(
                attributeList,
                (member, xAttributeList) => member.AddAttributeLists(xAttributeList) as T);

            return output;
        }

        public static AddNodeResult<T, AttributeListSyntax> AddAttributeList<T>(this T member)
            where T : MemberDeclarationSyntax
        {
            var attributeList = Instances.SyntaxGenerator.AttributeList();

            var output = member.AddAttributeList(attributeList);
            return output;
        }

        public static AddTokenResult<T> AddModifier<T>(this T member,
            Func<SyntaxToken> modifierConstructor,
            Func<SyntaxTokenList, int> modifierInsertionIndexProvider)
            where T : MemberDeclarationSyntax
        {
            // First, create and annotate the modifier token.
            var modifierToken = modifierConstructor()
                .Annotate(out var annotation)
                ;

            // Insert the modifier token at the proper place.
            member = member.ModifyModifiers(modifiers =>
            {
                // First find the position at which to add the modifier token.
                var indexForModifierToken = modifierInsertionIndexProvider(modifiers);

                // If the modifier is not the first modifier, prepend a separating space.
                if (IndexHelper.IsNotFirstIndex(indexForModifierToken))
                {
                    modifierToken = modifierToken.PrependSpace();
                }

                modifiers = modifiers.Insert(indexForModifierToken, modifierToken);

                return modifiers;
            });

            // Check that the member is spaced from it's last modifier.
            member = member.EnsureSeparatedFromLastModifier();

            var output = AddTokenResult.From(
                member,
                annotation);

            return output;
        }

        public static AddTokenResult<T> AddPublicModifier<T>(this T member)
            where T : MemberDeclarationSyntax
        {
            var output = member.AddModifier(
                Instances.SyntaxGenerator.Public,
                Instances.SyntaxOperator.GetIndexForPublicAccessModifer);

            return output;
        }

        public static AddTokenResult<T> MakePublicWithResult<T>(this T member)
            where T : MemberDeclarationSyntax
        {
            // If the member is already public, short-circuit.
            var hasPublicModifier = member.HasPublicModifier();
            if (hasPublicModifier)
            {
                member = member.AnnotateToken(
                    hasPublicModifier.Result,
                    out var annotation);

                var output = AddTokenResult.From(
                    member,
                    annotation);

                return output;
            }
            else
            {
                // If not already public, add the partial modifier.
                return member.AddPublicModifier();
            }
        }
    }
}
