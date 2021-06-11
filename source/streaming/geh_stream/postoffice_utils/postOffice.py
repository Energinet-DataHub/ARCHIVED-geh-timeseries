# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from geh_stream.postoffice_utils import Document_pb2
from geh_stream.DTOs.PostOfficeMessage import PostOfficeMessage
from azure.servicebus import ServiceBusClient, ServiceBusMessage
from google.protobuf.timestamp_pb2 import Timestamp
import datetime


class PostOffice:

    def __init__(self, connectionString, topic):

        self.TOPIC_NAME = topic

        # Create servicebus client
        self.servicebus_client = ServiceBusClient.from_connection_string(conn_str=connectionString, logging_enable=True)

    def __GenerateProtobufDocument(self, msg: PostOfficeMessage) -> Document_pb2:
        # Generate protobuf document
        document = Document_pb2.Document()
        document.content = msg.toJSON()
        document.type = "khsc doc"
        document.recipient = "khsc receip"
        document.version = "1"
        document.effectuationDate.GetCurrentTime()
        return document

    def send_single_message(self, msg: PostOfficeMessage):
        document = self.__GenerateProtobufDocument(msg)
        message = ServiceBusMessage(document.SerializeToString())
        # send the message to the queue
        sender = self.servicebus_client.get_topic_sender(topic_name=self.TOPIC_NAME)
        sender.send_messages(message)
        print("Done Sent a single message")
