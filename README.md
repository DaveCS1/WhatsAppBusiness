# WhatsApp Business & Tour Management Solution

A comprehensive tour management system with WhatsApp Business automation, AI-powered message processing, guide assignment management, SMS notifications, and advanced analytics dashboard.

## 🔗 **Live Applications & Documentation**

| Resource | URL | Status |
|----------|-----|--------|
| 🌐 **Blazor Admin Interface** | [https://whatsapp-business-client.azurewebsites.net](https://whatsapp-business-client.azurewebsites.net) | ✅ Live |
| 🔧 **API Documentation (Swagger)** | [https://whatsapp-business-api.azurewebsites.net/swagger](https://whatsapp-business-api.azurewebsites.net/swagger) | ✅ Live |
| 📡 **API Base URL** | `https://whatsapp-business-api.azurewebsites.net` | ⚠️ Not Browsable |
| 📂 **GitHub Repository** | [https://github.com/DaveCS1/WhatsAppBusiness](https://github.com/DaveCS1/WhatsAppBusiness) | ⚠️ Public Branch Outdated |

> **Note**: The API base URL is not directly browsable - use the Swagger documentation above to explore available endpoints.

> **⚠️ Security Notice**: The public GitHub repository contains an older version of the codebase. The current production version with all enterprise features documented below has not been pushed to the public branch due to security considerations (API keys, deployment configurations, etc.). Please contact the repository owner for access to the latest codebase.

## 🎯 **Project Status: Production Ready - Full Feature Complete**

### ✅ **Completed System - Enterprise-Grade Tour Management Platform**

This solution has evolved beyond simple WhatsApp automation into a complete tour management platform with:

#### **Phase 1: Database Foundation** ✅
- ✅ SQLite database with comprehensive schema (11 tables)
- ✅ Database initialization and verification system
- ✅ Core models: `Contact`, `Message`, `TourDetails`, `AutomatedResponseLog`, `MessageTemplates`
- ✅ Extended models: `Guides`, `Guests`, `TourAssignments`, `GuideReassignments`
- ✅ Sample data with 8 tour configurations and guide/guest records

#### **Phase 2: Core API Implementation** ✅
- ✅ **Data Access Layer**: `ChatRepository` with comprehensive Dapper methods (567 lines)
- ✅ **Business Services**:
  - `WhatsAppService` - Meta Graph API integration (213 lines)
  - `AiExtractionService` - Gemini AI integration with JSON schema (169 lines)
  - `TourPresetsService` - Smart tour matching logic (223 lines)
- ✅ **API Controllers**:
  - `WhatsappWebhookController` - Full webhook processing pipeline (373 lines)
  - `ChatController` - Complete REST API for admin interface (635 lines)
  - `TemplatesController` - Message template management (220 lines)
  - `ExportsController` - Comprehensive data export system (297 lines)
  - `PeopleController` - Guide and guest management (374 lines)
  - `AssignmentsController` - Tour assignment management (323 lines)
  - `ToursController` - Tour management operations (103 lines)
  - `TestController` - API testing and diagnostics (147 lines)

#### **Phase 3: Blazor Admin Interface** ✅
- ✅ **Complete Admin Dashboard**: Real-time management interface with 25+ pages
- ✅ **Core Pages**:
  - `Home.razor` - Dashboard with system statistics (387 lines)
  - `Chat.razor` - WhatsApp conversation management (467 lines)
  - `Analytics.razor` - Advanced analytics with interactive drill-down (1044 lines)
  - `Templates.razor` - Message template management (416 lines)
  - `Exports.razor` - Comprehensive data export system (662 lines)
- ✅ **People Management System**:
  - `People.razor` - Overview dashboard (190 lines)
  - `GuideManagement.razor` - Complete guide CRUD operations (330 lines)
  - `GuestManagement.razor` - Guest management system (389 lines)
  - `GuideDetails.razor` - Detailed guide profiles (266 lines)
  - `GuestDetails.razor` - Detailed guest profiles (292 lines)
- ✅ **Assignment Management**:
  - `AssignmentManagement.razor` - Tour assignment dashboard (423 lines)
  - `AssignmentDetails.razor` - Detailed assignment tracking (324 lines)
  - `QuickAssignmentModal.razor` - Rapid assignment creation (665 lines)
  - `ReassignmentModal.razor` - Guide reassignment workflow (397 lines)
  - `GuideScheduleModal.razor` - Schedule management (337 lines)
- ✅ **Communication System**:
  - `TwilioSms.razor` - SMS messaging interface (462 lines)
  - `CheckFrontApi.razor` - External API integration (148 lines)

#### **Phase 4: Advanced Features** ✅
- ✅ **Template System**: Dynamic message templates with placeholder variables
- ✅ **Export Capabilities**: 5 export types with multiple formats (CSV, JSON, Text)
- ✅ **Advanced Analytics**: Performance tracking, business intelligence, interactive drill-down
- ✅ **People Management**: Complete CRUD for guides and guests with specialties tracking
- ✅ **Assignment System**: Tour assignment with reassignment workflow and confirmation tracking
- ✅ **SMS Integration**: Twilio integration for guide notifications and confirmations
- ✅ **Production Deployment**: Azure-ready with CI/CD pipeline

### 🚀 **Current System Capabilities**

The complete system now provides:

#### **WhatsApp Business Automation**
- ✅ **AI-Powered Message Processing**: Automatic extraction of customer intent using Gemini 2.0 Flash
- ✅ **Automated Tour Booking Responses**: Intelligent matching and instant responses
- ✅ **Message Template Management**: Customizable templates with dynamic content
- ✅ **Real-time Chat Interface**: Complete conversation management

#### **Tour Management Platform**
- ✅ **Guide Management**: Complete CRUD operations with specialties, contact info, and availability
- ✅ **Guest Management**: Customer profiles with booking history and preferences
- ✅ **Tour Assignment System**: Assign guides to tours with full audit trail
- ✅ **Reassignment Workflow**: Handle guide changes with confirmation tracking
- ✅ **Schedule Management**: Visual guide schedules and availability tracking

#### **Communication & Notifications**
- ✅ **SMS Integration**: Twilio-powered SMS for guide notifications
- ✅ **Confirmation System**: Guide assignment confirmations via SMS with web links
- ✅ **Multi-channel Communication**: WhatsApp, SMS, and in-person confirmation tracking

#### **Analytics & Reporting**
- ✅ **Advanced Analytics Dashboard**: Performance metrics, business intelligence
- ✅ **Interactive Log Analysis**: Detailed response inspection with modal dialogs
- ✅ **Export System**: 5 comprehensive export types with multiple formats
- ✅ **System Monitoring**: Real-time API connectivity and database health

#### **Production-Ready Infrastructure**
- ✅ **Azure App Service Integration**: Fully configured for cloud deployment
- ✅ **GitHub Actions CI/CD**: Automatic deployment pipeline
- ✅ **Professional UI**: Bootstrap 5 with responsive design and modern UX
- ✅ **Error Handling**: Comprehensive error management and recovery

## 🏗️ **Architecture Overview**

### **Solution Structure**
```
WhatsAppBusiness/
├── WhatsAppBusinessAPI/           # ASP.NET Core Web API (8 Controllers, 4 Services)
│   ├── Controllers/               # API Controllers
│   │   ├── WhatsappWebhookController.cs    # WhatsApp webhook processing
│   │   ├── ChatController.cs               # Chat management API
│   │   ├── TemplatesController.cs          # Message templates
│   │   ├── ExportsController.cs            # Data export system
│   │   ├── PeopleController.cs             # Guide/Guest management
│   │   ├── AssignmentsController.cs        # Tour assignments
│   │   ├── ToursController.cs              # Tour management
│   │   └── TestController.cs               # API testing
│   ├── Services/                  # Business Logic Services
│   │   ├── WhatsAppService.cs              # Meta Graph API integration
│   │   ├── AiExtractionService.cs          # Gemini AI processing
│   │   ├── TourPresetsService.cs           # Tour matching logic
│   │   └── DbInitializer.cs                # Database setup
│   ├── Repositories/              # Data Access Layer
│   │   └── ChatRepository.cs               # Comprehensive data access (567 lines)
│   ├── Models/                    # Data Models (11 model classes)
│   └── Data/                      # Database & Scripts
└── WhatsAppBusinessBlazorClient/  # Blazor Server Admin UI (25+ Pages)
    ├── Components/Pages/          # Blazor Pages
    │   ├── Home.razor                      # Dashboard
    │   ├── Chat.razor                      # WhatsApp management
    │   ├── Analytics.razor                 # Advanced analytics
    │   ├── Templates.razor                 # Template management
    │   ├── Exports.razor                   # Data exports
    │   ├── People.razor                    # People overview
    │   ├── GuideManagement.razor           # Guide CRUD
    │   ├── GuestManagement.razor           # Guest CRUD
    │   ├── AssignmentManagement.razor      # Assignment dashboard
    │   ├── TwilioSms.razor                 # SMS interface
    │   └── [20+ additional pages]
    ├── Services/                  # Client-side Services
    │   ├── ApiService.cs                   # HTTP client (490 lines)
    │   ├── TwilioService.cs                # SMS service
    │   └── ToastService.cs                 # UI notifications
    └── Models/                    # Client-side Models
```

### **Database Schema (11 Tables)**
- **Core WhatsApp**: `Contacts`, `Messages`, `AutomatedResponseLog`, `MessageTemplates`
- **Tour Management**: `TourDetails`, `TourAssignments`, `GuideReassignments`
- **People Management**: `Guides`, `Guests`
- **System**: `SystemSettings`

### **Key Technologies**
- **Backend**: ASP.NET Core 8.0, SQLite, Dapper
- **AI Integration**: Google Gemini 2.0 Flash with JSON Schema
- **WhatsApp**: Meta Graph API v19.0
- **SMS**: Twilio API integration
- **Frontend**: Blazor Server, Bootstrap 5
- **Database**: SQLite with comprehensive logging and analytics
- **Export**: CSV, JSON, and formatted text generation
- **Deployment**: Azure App Services with GitHub Actions CI/CD

## 🔧 **Configuration Required**

### **API Configuration (`appsettings.json`)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/whatsapp.db"
  },
  "WhatsApp": {
    "ApiToken": "YOUR_WHATSAPP_API_TOKEN",
    "AppSecret": "YOUR_WHATSAPP_APP_SECRET", 
    "VerifyToken": "YOUR_WHATSAPP_VERIFY_TOKEN",
    "PhoneNumberId": "YOUR_WHATSAPP_PHONE_NUMBER_ID",
    "CompanyName": "NYC Adventure Tours"
  },
  "AiApi": {
    "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models/",
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-2.0-flash"
  }
}
```

### **Blazor Client Configuration (`appsettings.json`)**
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5260"
  },
  "TwilioSettings": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "FromPhoneNumber": "YOUR_TWILIO_PHONE_NUMBER",
    "IsTestMode": false
  }
}
```

## 📊 **Database Schema Details**

### **Core Tables**
- **Contacts**: WhatsApp contacts with AI-extracted user information
- **Messages**: All WhatsApp messages (incoming/outgoing) with status tracking
- **TourDetails**: Preset tour information for automated responses
- **AutomatedResponseLog**: Comprehensive logging of all automated responses
- **MessageTemplates**: Customizable message templates with dynamic placeholders

### **People Management Tables**
- **Guides**: Tour guide profiles with specialties and contact information
- **Guests**: Customer profiles with booking history
- **TourAssignments**: Tour-to-guide assignments with reassignment tracking
- **GuideReassignments**: Detailed reassignment workflow with confirmation status

### **Sample Data Included**
- 8 sample tour configurations (Walking, Food, Historical, Art tours)
- Sample guide profiles with different specialties
- Sample guest records with booking history
- 5 default message templates with placeholder variables
- System settings for configuration management

## 📋 **API Endpoints**

### **WhatsApp Webhook Endpoints**
- `GET /api/whatsapp/webhook` - Webhook verification
- `POST /api/whatsapp/webhook` - Message processing

### **Chat Management Endpoints**
- `GET /api/chat/contacts` - Get all contacts
- `GET /api/chat/messages/{contactId}` - Get messages for contact
- `POST /api/chat/send` - Send manual message
- `GET /api/chat/logs` - Get automated response logs
- `GET /api/chat/stats` - Get system statistics

### **People Management Endpoints**
- `GET /api/people/guides` - Get all guides
- `POST /api/people/guides` - Create new guide
- `PUT /api/people/guides/{id}` - Update guide
- `DELETE /api/people/guides/{id}` - Delete guide
- `GET /api/people/guests` - Get all guests
- `POST /api/people/guests` - Create new guest

### **Assignment Management Endpoints**
- `GET /api/assignments` - Get all tour assignments
- `POST /api/assignments` - Create new assignment
- `PUT /api/assignments/{id}` - Update assignment
- `POST /api/assignments/{id}/reassign` - Reassign guide
- `GET /api/assignments/guide/{guideId}` - Get assignments for guide

### **Template Management Endpoints**
- `GET /api/templates` - Get all message templates
- `POST /api/templates` - Create new template
- `PUT /api/templates/{id}` - Update existing template
- `DELETE /api/templates/{id}` - Delete template
- `POST /api/templates/{id}/preview` - Preview template with sample data

### **Export Endpoints**
- `GET /api/exports/tours` - Export tour booking data
- `GET /api/exports/contacts` - Export customer contact information
- `GET /api/exports/messages` - Export WhatsApp conversation history
- `GET /api/exports/guide-messages` - Export formatted guide communications
- `GET /api/exports/automated-responses` - Export AI processing analytics

## 🤖 **AI Processing Pipeline**

1. **Message Reception**: WhatsApp webhook receives incoming message
2. **AI Extraction**: Gemini AI extracts structured data (name, tour type, date, time)
3. **Tour Matching**: Smart matching algorithm finds best tour option
4. **Template Processing**: Dynamic template system generates personalized response
5. **Response Generation**: Template-based response with tour details and placeholders
6. **Message Sending**: Automated response sent via WhatsApp API
7. **Comprehensive Logging**: All steps logged for analytics and export

## 📈 **Analytics & Export Features**

### **Advanced Analytics Dashboard**
- **Processing Time Tracking**: AI call duration, total processing time with performance ratings
- **Success/Failure Rates**: Automated response success tracking with visual progress bars
- **Interactive Log Analysis**: Click any response entry for detailed modal with full message content
- **Performance Insights**: Efficiency ratings, speed analysis, and activity level monitoring
- **Contact Management**: AI-extracted user information storage with conversation history
- **Message History**: Complete conversation history with drill-down capabilities
- **Template Usage**: Track which templates are most effective with usage analytics

### **People Management Analytics**
- **Guide Performance**: Track guide assignments, confirmations, and availability
- **Guest Analytics**: Booking patterns, preferences, and customer lifetime value
- **Assignment Metrics**: Success rates, reassignment frequency, and confirmation times
- **Communication Tracking**: SMS delivery rates, confirmation response times

### **Export System (5 Types)**
1. **Tours Export**: Complete tour booking data with participant details
2. **Contacts Export**: Customer information with tour preferences and contact details
3. **Messages Export**: Full WhatsApp conversation history with metadata
4. **Guide Messages Export**: Formatted communications for easy forwarding to tour guides
5. **Automated Responses Export**: AI processing analytics and system performance data

### **Export Formats**
- **CSV**: Spreadsheet-compatible format for data analysis
- **JSON**: Structured data for system integration
- **Formatted Text**: Human-readable format for direct use

## 🎨 **Admin Interface Features**

### **Dashboard Pages**
- **Home Dashboard**: System overview with real-time statistics and health monitoring
- **Chat Management**: Real-time WhatsApp conversation monitoring and manual messaging
- **Advanced Analytics**: System performance metrics, business intelligence, and interactive log analysis
- **Templates**: Message template creation and management with live preview
- **Exports**: Comprehensive data export with multiple format options

### **People Management Interface**
- **Guide Management**: Complete CRUD operations with specialties, availability, and performance tracking
- **Guest Management**: Customer profiles with booking history and preferences
- **Assignment Dashboard**: Visual tour assignment management with drag-and-drop interface
- **Schedule Management**: Guide availability and tour scheduling with calendar views

### **Communication Interface**
- **SMS Dashboard**: Send SMS notifications to guides with delivery tracking
- **Confirmation System**: Track guide confirmations with web-based confirmation links
- **Multi-channel Tracking**: Monitor communications across WhatsApp, SMS, and in-person channels

### **Interactive Features**
- **Clickable Log Entries**: Click any automated response in analytics for detailed information
- **Comprehensive Modal Dialogs**: Full-screen dialogs with complete response details and performance metrics
- **Copy to Clipboard**: One-click copying of response text for easy sharing or analysis
- **Individual Log Export**: Export detailed information for specific automated responses
- **Performance Ratings**: Real-time efficiency and speed ratings for each response
- **JSON Data Formatting**: Properly formatted AI-extracted data for easy reading

### **User Experience**
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Real-time Updates**: Live status indicators and automatic refresh
- **Professional UI**: Modern Bootstrap 5 design with intuitive navigation
- **Interactive Elements**: Hover effects, clickable rows, and smooth modal transitions
- **Error Handling**: Comprehensive error messages and recovery options
- **Toast Notifications**: Real-time feedback for user actions

## 🚀 **Production Deployment**

### **Azure App Services Configuration:**
- **API**: `https://whatsapp-business-api.azurewebsites.net`
- **Client**: `https://whatsapp-business-client.azurewebsites.net`

### **Deployment Features:**
- ✅ **GitHub Actions CI/CD**: Automatic deployment on `main` branch push
- ✅ **PowerShell Scripts**: Local deployment automation (`deploy-azure.ps1`)
- ✅ **Visual Studio Integration**: Direct publish from IDE with publish profiles
- ✅ **Production Configurations**: Optimized for Azure Windows App Services
- ✅ **Environment-specific Settings**: Separate configurations for Development/Production

### **GitHub Actions Setup:**
Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

- **`AZURE_WEBAPP_API_PUBLISH_PROFILE`**: Content from `docs/whatsapp-business-api.PublishSettings`
- **`AZURE_WEBAPP_CLIENT_PUBLISH_PROFILE`**: Content from `docs/whatsapp-business-client.PublishSettings`

### **Quick Deployment:**
```powershell
# Deploy both API and Client
.\deploy-azure.ps1

# Deploy specific service
.\deploy-azure.ps1 -ApiOnly
.\deploy-azure.ps1 -ClientOnly

# Build locally
.\build.ps1
.\build.ps1 -Configuration Release
```

### **Production Configuration:**
Update these values in production `appsettings.json` files:
- WhatsApp API credentials (Token, Secret, Verify Token, Phone Number ID)
- Gemini AI API key
- Twilio credentials (Account SID, Auth Token, Phone Number)
- Any environment-specific settings

## 🎯 **Business Value Delivered**

### **Operational Efficiency**
- **24/7 Automated Responses**: Never miss a booking opportunity
- **Instant Customer Service**: Immediate responses to tour inquiries
- **Scalable Operations**: Handle unlimited inquiries without staff increase
- **Consistent Quality**: Every customer receives professional, detailed information

### **Tour Management Automation**
- **Guide Assignment System**: Streamlined tour-to-guide assignments with full audit trail
- **Reassignment Workflow**: Handle guide changes with confirmation tracking
- **SMS Notifications**: Automatic guide notifications with confirmation links
- **Schedule Management**: Visual guide availability and conflict resolution

### **Data Management & Analytics**
- **Complete Customer Profiles**: AI-extracted information with conversation history
- **Guide Performance Tracking**: Assignment success rates, confirmation times, availability
- **Export Capabilities**: Easy data integration with existing business systems
- **Performance Tracking**: Monitor system effectiveness and customer satisfaction
- **Business Intelligence**: Identify trends and optimize tour offerings

### **Communication Management**
- **Multi-channel Integration**: WhatsApp, SMS, and in-person communication tracking
- **Customizable Templates**: Maintain brand consistency across all communications
- **Dynamic Content**: Personalized responses with customer-specific information
- **Guide Integration**: Formatted communications for easy forwarding to tour guides
- **Confirmation System**: Web-based confirmation links with automatic tracking

### **Enterprise Features**
- **Comprehensive Audit Trail**: Track all changes, assignments, and communications
- **Role-based Access**: Different access levels for different user types
- **Real-time Monitoring**: System health, API connectivity, and performance metrics
- **Scalable Architecture**: Designed to handle growth in tours, guides, and customers
- **Production-ready Deployment**: Azure integration with CI/CD pipeline

**See [`BUILD.md`](BUILD.md) for complete deployment instructions.**
