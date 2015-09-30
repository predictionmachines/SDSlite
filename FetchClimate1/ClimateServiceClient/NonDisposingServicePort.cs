using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
#if FC_WCFCLIENT
    public class NonDisposingServicePort : IServicePort
    {
        private IServicePort innerServicePort;

        public NonDisposingServicePort(IServicePort servicePort)
        {
            if (servicePort == null)
                throw new ArgumentNullException("servicePort");
            this.innerServicePort = servicePort;
        }

        public IServicePort ServicePort { get { return innerServicePort; } }

        #region IServicePort Members

        public void PostSubscribeRequest(INotifyPort message, IResponsePort responsePort)
        {
            innerServicePort.PostSubscribeRequest(message, responsePort);
        }

        public void PostUnsubscribeRequest(int subscriptionId, IResponsePort responsePort)
        {
            innerServicePort.PostUnsubscribeRequest(subscriptionId, responsePort);
        }

        public void PostGetSchemaRequest(int dataSetId, ISchemaResponsePort responsePort)
        {
            innerServicePort.PostGetSchemaRequest(dataSetId, responsePort);
        }

        public void PostGetMultipleDataRequest(Pipeline.GetDataRequest[] message, IMultipleDataResponsePort responsePort)
        {
            innerServicePort.PostGetMultipleDataRequest(message, responsePort);
        }

        public void PostGetDataRequest(Pipeline.GetDataRequest message, IDataResponsePort responsePort)
        {
            innerServicePort.PostGetDataRequest(message, responsePort);
        }

        public void PostUpdateRequest(Pipeline.UpdateRequest message, IUpdateResponsePort responsePort)
        {
            innerServicePort.PostUpdateRequest(message, responsePort);
        }

        public void PostAttachRequest(string constructionString, IResponsePort responsePort)
        {
            innerServicePort.PostAttachRequest(constructionString, responsePort);
        }

        public void PostDetachRequest(int dataSetId, IResponsePort responsePort)
        {
            innerServicePort.PostDetachRequest(dataSetId, responsePort);
        }

        #endregion
    }
#endif
}
