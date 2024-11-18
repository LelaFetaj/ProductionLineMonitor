# ProductionLineMQTTClient

**Task object**

Development of an MQTT client with GUI with 2 pages for the collection of some infos regarding a production line.

Line is composed of 5 machines called M1, M2, M3, M4, M5.

 

**Main page that shows:**

1. Company logo, machine info; (picture and info coming from RSA);
2. Line status: whether it is in production or not(downtime); (sub/MQTT);
3. Cycle times of each machines (M1, M2, M3, M4, M5); (sub/MQTT);
 

**Productivity page that shows:**

1. Line production piece counter; (sub/MQTT);
2. Waste piece counter; (sub/MQTT);
3. Target piece counter; (sub/MQTT);
4. Downtime (to be calculated-info coming from RSA);
5. OEE percentage (to be calculated-formula coming from RSA);
