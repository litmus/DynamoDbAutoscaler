We no longer use or maintain this project.  Please reach out to hello@limtus.com if you are interested in taking ownership.

# DynamoDbAutoscaler
Provides auto scaling for AWS DynamoDB within a .NET library.  Inspired by [sebdah's dynamic-dynamodb project](https://github.com/sebdah/dynamic-dynamodb).

[![Build status](https://ci.appveyor.com/api/projects/status/8m542idys5e959xs?svg=true)](https://ci.appveyor.com/project/brianfeucht/dynamodbautoscaler)

# Nuget
Nuget package is available at https://www.nuget.org/packages/DynamoDbAutoscaler/

# Usage
To use in a service with a [simple local json configuration](https://github.com/litmus/DynamoDbAutoscaler/blob/master/autoscaling.json) create an instance DynamoDbAutoscalerIntervalProvisioner and call Start on service start.  The autoscaler will load the file at ./autoscaling.json as its configuration for throughput provisioning and make adjustments every 5 minutes.  

# Logging
This code relies on Serilog to provide logging.  There is currently an [open issue](https://github.com/litmus/DynamoDbAutoscaler/issues/1) to make this dependency optional

# Permissions 
By default this code will use the IAM profile or AWS keys provided in your app.config and verify that your provisioned DynamoDb throughput levels are within the configured values.  Your IAM user will need read rights for Cloudwatch (to read provisioned, consumed, and throttled values) and rights to edit provisioned read/write units on the target DynamoDb tables.  Specifically you will need:
* cloudwatch:GetMetricStatistics
* dynamodb:DescribeTable
* dynamodb:ListTables
* dynamodb:UpdateTable

# Customization
Most everything in this project is an interface.  To use your implementation simply override the default implementations in the [Autoscaler constructor](https://github.com/litmus/DynamoDbAutoscaler/blob/master/DynamoDbAutoScaler/Autoscaler.cs).

# Contributing
Please feel free to submit pull requests or bugs within Github.  
