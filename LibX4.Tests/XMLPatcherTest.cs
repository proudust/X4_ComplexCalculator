using System;
using LibX4.Xml;
using Xunit;

namespace LibX4.Tests
{
    /// <summary>
    /// <see cref="XMLPatcher"/> のテストクラス
    /// 参考: <a href="https://github.com/Shoobx">Shoobx/xmldiff</a>
    /// </summary>
    public class XMLPatcherTest
    {
        [Fact]
        public void AddingAnElement()
        {
            var baseXml = "<doc><bar /></doc>".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""doc"">
                    <foo id=""ert4773"">This is a new child</foo>
                </add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <bar />
              <foo id=""ert4773"">This is a new child</foo>
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingAnAttribute()
        {
            var baseXml = @"<doc><foo id=""dqs3662"" /><foo id=""ert4773"" /></doc>".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""doc/foo[@id='ert4773']"" type=""@user"">Bob</add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <foo id=""dqs3662"" />
              <foo id=""ert4773"" user=""Bob"" />
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingAPrefixedNamespaceDeclaration()
        {
            var baseXml = "<doc />".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""doc"" type=""namespace::pref"">urn:ns:xxx</add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            const string patchedXml = @"<doc xmlns:pref=""urn:ns:xxx"" />";
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingNodeWithThePosAttribute()
        {
            var baseXml = @"<doc><foo id=""dqs3662"" /><foo id=""ert4773"" /></doc>".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""doc/foo[@id='ert4773']"" pos=""before"">
                    <!-- comment -->
                </add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <foo id=""dqs3662"" />
              <!-- comment -->
              <foo id=""ert4773"" />
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingMultipleNodes_IfLastVhildIsWhiteSpaceTextNode()
        {
            var baseXml = "<doc>This is a text node.</doc>".ToXDocument();
            var patchXml = @"
            <diff><add sel=""doc"">
              <foo id=""ert4773"">This is a new child</foo></add></diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>This is a text node.<foo id=""ert4773"">This is a new child</foo></doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingMultipleNodes_IfLastVhildIsOther()
        {
            var baseXml = @"<doc><foo id=""dqs3662"" /></doc>".ToXDocument();
            var patchXml = @"
            <diff><add sel=""doc"">
              <foo id=""ert4773"">This is a new child</foo></add></diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <foo id=""dqs3662"" />
              <foo id=""ert4773"">This is a new child</foo>
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingMultipleNodes_PosAfter()
        {
            var baseXml = "<doc><foo>This is <bar /> a text node.</foo></doc>".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""*/foo/text()[2]"" pos=""after"">new<bar />elem</add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <foo>This is <bar /> a text node.new<bar />elem</foo>
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void AddingMultipleNodes_PosBefore()
        {
            var baseXml = "<doc><foo>This is <bar /> a text node.</foo></doc>".ToXDocument();
            var patchXml = @"
            <diff>
                <add sel=""*/foo/text()[2]"" pos=""before"">new<bar />elem</add>
            </diff>
            ".ToXDocument();
            baseXml.MergeXML(patchXml);
            var patchedXml = @"
            <doc>
              <foo>This is <bar />new<bar />elem a text node.</foo>
            </doc>
            ".TrimIndent();
            Assert.Equal(patchedXml, baseXml.ToString());
        }


        [Fact]
        public void ReplacingAnElement()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void ReplacingAnAttributeValue()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void ReplacingANamespaceDeclarationUri()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void ReplacingACommentNode()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void ReplacingAProcessingInstructionNode()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void ReplacingATextNode()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingAnElement()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingAnAttribute()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingAPrefixedNamespaceDeclaration()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingACommentNode()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingAProcessingInstructionNode()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public void RemovingATextNode()
        {
            throw new NotImplementedException();
        }
    }
}
