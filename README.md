# SimpleSolutions

A set of solutions for Power Platform which introduce basic "business" funcitonality (email templates, email scheduling, support requests, payments, orders, etc)

## v 1.0.0.1

## Solution files: SimpleMarketing_1_0_0_1.zip and SimpleMarketing_1_0_0_1_managed.zip (unmanaged and managed versions)

## Azure Functions are required (Look here for the sources: https://github.com/ashlega/SimpleSolutions/tree/main/SimpleMarketing/SimpleMarketingFunctions)

## Functionality

- Create email templates using handlebars syntax
- Use SQL queries (over TDS endpoint) to get data from Dataverse for those templates above
- Schedule email delivery
- Use Exchange SMTP when sending emails from the shared mailbox (this is also to control email delivery and avoid outgoing exchange “spam” filters)
- Create support requests for form submissions using a simple Power Automate flow with an HTTP trigger
- Automatically create support requests for incoming emails
- And, of course, use Dataverse Outlook integration for outgoing emails when replying to the support requests

