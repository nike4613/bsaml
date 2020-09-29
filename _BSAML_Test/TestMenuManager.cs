using Zenject;
using UnityEngine;
using System.Collections;
using IPA.Utilities;
using HMUI;

namespace _BSAML_Test
{
    internal class TestMenuManager : MonoBehaviour
    {
        [Inject] private MainFlowCoordinator mainFlowCoordinator = null!;
        [Inject] private PresenterFC presenterFC = null!;

        public IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            mainFlowCoordinator.InvokeMethod<object, FlowCoordinator>("PresentFlowCoordinator", presenterFC, null, false, false);
        }
    }
}
