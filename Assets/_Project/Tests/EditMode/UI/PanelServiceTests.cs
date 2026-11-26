using System;
using Novent.Tests.Support;
using Novent.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novent.Tests.UI
{
    public class PanelServiceTests
    {
        private GameObject _parent;
        private GameObject _testPanelPrefab;
        private GameObject _otherPanelPrefab;
        private PanelRegistry _registry;
        private PanelService _panels;

        [SetUp]
        public void SetUp()
        {
            _parent = new GameObject("Canvas");
            _testPanelPrefab = new GameObject("TestPanel", typeof(CanvasGroup), typeof(TestPanel));
            _otherPanelPrefab = new GameObject("OtherTestPanel", typeof(CanvasGroup), typeof(OtherTestPanel));
            _registry = ScriptableObject.CreateInstance<PanelRegistry>();
            var serialized = new SerializedObject(_registry);
            var list = serialized.FindProperty("_panels");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = _testPanelPrefab.GetComponent<TestPanel>();
            list.GetArrayElementAtIndex(1).objectReferenceValue = _otherPanelPrefab.GetComponent<OtherTestPanel>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _panels = new PanelService(_registry, _parent.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parent);
            Object.DestroyImmediate(_testPanelPrefab);
            Object.DestroyImmediate(_otherPanelPrefab);
            Object.DestroyImmediate(_registry);
        }

        [Test]
        public void Open_ShowsPanelOnTop()
        {
            var panel = _panels.Open<TestPanel>();

            Assert.That(panel.IsOpen, Is.True);
            Assert.That(_panels.Top, Is.SameAs(panel));
            Assert.That(panel.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);
            Assert.That(panel.transform.parent, Is.SameAs(_parent.transform));
        }

        [Test]
        public void Open_Twice_ReusesInstance()
        {
            var first = _panels.Open<TestPanel>();

            var second = _panels.Open<TestPanel>();

            Assert.That(second, Is.SameAs(first));
            Assert.That(_parent.transform.childCount, Is.EqualTo(1));
        }

        [Test]
        public void Back_ClosesTopAndRevealsPanelBelow()
        {
            var below = _panels.Open<TestPanel>();
            var top = _panels.Open<OtherTestPanel>();

            var consumed = _panels.Back();

            Assert.That(consumed, Is.True);
            Assert.That(top.IsOpen, Is.False);
            Assert.That(_panels.Top, Is.SameAs(below));
        }

        [Test]
        public void Back_WithNoPanels_NotConsumed()
        {
            Assert.That(_panels.Back(), Is.False);
        }

        [Test]
        public void Back_TopDestroyedElsewhere_ClosesNextPanel()
        {
            var below = _panels.Open<TestPanel>();
            var top = _panels.Open<OtherTestPanel>();
            Object.DestroyImmediate(top.gameObject);

            var consumed = _panels.Back();

            Assert.That(consumed, Is.True);
            Assert.That(below.IsOpen, Is.False);
        }

        [Test]
        public void Open_PanelMissingFromRegistry_ThrowsWithName()
        {
            var error = Assert.Throws<InvalidOperationException>(() => _panels.Open<UnregisteredTestPanel>());

            StringAssert.Contains(nameof(UnregisteredTestPanel), error.Message);
        }
    }
}
