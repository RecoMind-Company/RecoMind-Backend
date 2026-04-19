# ?? Flutter SignalR Integration - Complete Guide

## Complete Implementation Guide for SignalR Comment Hub

This is a **production-ready, complete-in-one-file guide** for integrating your Flutter app with the .NET 8 SignalR Comment Hub. Everything you need is here.

---

## ?? Table of Contents

1. [Quick Start (5 min)](#quick-start)
2. [Architecture Overview](#architecture-overview)
3. [Required Packages](#required-packages)
4. [Complete Implementation](#complete-implementation)
5. [API Reference](#api-reference)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## ?? Quick Start

### Step 1: Create Flutter Project
```bash
flutter create comment_hub_app
cd comment_hub_app
```

### Step 2: Update pubspec.yaml
```yaml
dependencies:
  flutter:
    sdk: flutter
  signalr_netcore: ^1.3.5
  flutter_bloc: ^8.1.3
  bloc: ^8.1.2
  http: ^1.1.0
  intl: ^0.19.0
  uuid: ^4.0.0
  equatable: ^2.0.5
  logging: ^1.2.0
  flutter_dotenv: ^5.1.0
  json_annotation: ^4.8.1

dev_dependencies:
  flutter_test:
    sdk: flutter
  build_runner: ^2.4.6
  json_serializable: ^6.7.1
```

### Step 3: Run Commands
```bash
flutter pub get
flutter pub run build_runner build
```

### Step 4: Get Your JWT Token
```bash
# From your backend's token endpoint
# Example: POST /api/auth/login
# Response: { "token": "eyJhbGc..." }
```

### Step 5: Run the App
```bash
flutter run
```

---

## ??? Architecture Overview

```
???????????????????????????????????????????????????
?              Flutter Application                ?
???????????????????????????????????????????????????
?  UI Layer (Pages, Widgets)                      ?
?  ?? CommentPage                                 ?
?  ?? CommentCard, DateSeparator, CommentInput   ?
???????????????????????????????????????????????????
?  State Management (BLoC)                        ?
?  ?? CommentBloc (handles events/states)        ?
?  ?? CommentEvent, CommentState                 ?
???????????????????????????????????????????????????
?  Services Layer                                 ?
?  ?? SignalRService (WebSocket communication)   ?
?  ?? Stream controllers for real-time events    ?
???????????????????????????????????????????????????
?  Models                                         ?
?  ?? CommentModel (with timestamp methods)      ?
?  ?? AddCommentDto, UpdateCommentDto            ?
???????????????????????????????????????????????????
           ? (WebSocket over TCP)
???????????????????????????????????????????????????
?      SignalR Hub (.NET 8 Backend)               ?
?  Endpoint: wss://localhost:7264/hubs/comments  ?
???????????????????????????????????????????????????
?  Methods:                                       ?
?  - CreateComment(AddCommentDto)                ?
?  - EditComment(UpdateCommentDto)               ?
?  - DeleteComment(commentId)                    ?
???????????????????????????????????????????????????
?  Events:                                        ?
?  - ReceiveComment(CommentDto)                  ?
?  - ReceiveEditedComment(CommentDto)            ?
?  - ReceiveDeletedComment(commentId)            ?
?  - ReceiveErrors(errors[])                     ?
???????????????????????????????????????????????????
```

---

## ?? Required Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `signalr_netcore` | ^1.3.5 | SignalR client for real-time communication |
| `flutter_bloc` | ^8.1.3 | BLoC widgets and state management |
| `bloc` | ^8.1.2 | BLoC core library |
| `equatable` | ^2.0.5 | Equality comparison for Dart objects |
| `intl` | ^0.19.0 | Date/time formatting |
| `uuid` | ^4.0.0 | Generate unique IDs |
| `http` | ^1.1.0 | HTTP client (optional, for REST calls) |
| `json_annotation` | ^4.8.1 | JSON serialization helpers |
| `logging` | ^1.2.0 | Debug logging |
| `flutter_dotenv` | ^5.1.0 | Load environment variables |

---

## ?? Complete Implementation

### Part 1: Models

#### lib/models/comment_model.dart

```dart
import 'package:equatable/equatable.dart';
import 'package:intl/intl.dart';

class CommentModel extends Equatable {
  final String id;
  final String userComment;
  final String userId;
  final String planId;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const CommentModel({
    required this.id,
    required this.userComment,
    required this.userId,
    required this.planId,
    required this.createdAt,
    this.updatedAt,
  });

  /// Check if comment can be edited (within 5 minute window)
  bool canEdit() {
    final now = DateTime.now();
    final diffMinutes = now.difference(createdAt).inMinutes;
    return diffMinutes < 5;
  }

  /// Get time-only display in HH:MM format (WhatsApp style)
  String getTimeDisplay() {
    final displayTime = updatedAt ?? createdAt;
    final formatter = DateFormat('HH:mm');
    return formatter.format(displayTime);
  }

  /// Get date display (Today, Yesterday, or date)
  String getDateDisplay() {
    final date = createdAt;
    final today = DateTime.now();
    final yesterday = today.subtract(const Duration(days: 1));

    if (date.year == today.year &&
        date.month == today.month &&
        date.day == today.day) {
      return 'Today';
    } else if (date.year == yesterday.year &&
        date.month == yesterday.month &&
        date.day == yesterday.day) {
      return 'Yesterday';
    } else {
      final formatter = DateFormat('MMM d');
      return formatter.format(date);
    }
  }

  /// Check if comment has been edited
  bool get isUpdated => updatedAt != null;

  /// Get user ID first 3 characters in uppercase
  String get userIdShort => userId.substring(0, 3).toUpperCase();

  /// Copy with method for immutability
  CommentModel copyWith({
    String? id,
    String? userComment,
    String? userId,
    String? planId,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return CommentModel(
      id: id ?? this.id,
      userComment: userComment ?? this.userComment,
      userId: userId ?? this.userId,
      planId: planId ?? this.planId,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  /// Convert from JSON (from server)
  factory CommentModel.fromJson(Map<String, dynamic> json) {
    return CommentModel(
      id: json['id'] as String,
      userComment: json['userComment'] as String,
      userId: json['userId'] as String,
      planId: json['planId'] as String? ?? '',
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null
          ? DateTime.parse(json['updatedAt'] as String)
          : null,
    );
  }

  /// Convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userComment': userComment,
      'userId': userId,
      'planId': planId,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [id, userComment, userId, planId, createdAt, updatedAt];
}
```

#### lib/models/dtos.dart

```dart
/// DTO for creating a new comment
class AddCommentDto {
  final String userComment;
  String? userId;
  String? planId;

  AddCommentDto({
    required this.userComment,
    this.userId,
    this.planId,
  });

  Map<String, dynamic> toJson() => {
    'userComment': userComment,
    'userId': userId,
    'planId': planId,
  };
}

/// DTO for updating an existing comment
class UpdateCommentDto {
  final String commentId;
  final String userComment;
  String? userId;

  UpdateCommentDto({
    required this.commentId,
    required this.userComment,
    this.userId,
  });

  Map<String, dynamic> toJson() => {
    'commentId': commentId,
    'userComment': userComment,
    'userId': userId,
  };
}
```

---

### Part 2: SignalR Service

#### lib/services/signalr_service.dart

```dart
import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:logging/logging.dart';
import '../models/comment_model.dart';

/// SignalR Service for real-time comment communication
class SignalRService {
  late HubConnection _hubConnection;
  final String _baseUrl;
  final String _token;
  final String _planId;
  final _logger = Logger('SignalRService');

  // Stream controllers for broadcasting events
  final StreamController<CommentModel> commentCreatedStream = 
      StreamController.broadcast();
  final StreamController<CommentModel> commentEditedStream = 
      StreamController.broadcast();
  final StreamController<String> commentDeletedStream = 
      StreamController.broadcast();
  final StreamController<String> connectionStatusStream = 
      StreamController.broadcast();
  final StreamController<List<String>> errorsStream = 
      StreamController.broadcast();

  /// Check if connected to SignalR
  bool get isConnected => _hubConnection.state == HubConnectionState.Connected;

  /// Constructor
  SignalRService({
    required String baseUrl,
    required String token,
    required String planId,
  })  : _baseUrl = baseUrl,
        _token = token,
        _planId = planId;

  /// Initialize and connect to SignalR Hub
  /// 
  /// Throws: Exception if connection fails
  Future<void> connect() async {
    try {
      final url = '$_baseUrl/hubs/comments?planId=${Uri.encodeComponent(_planId)}';
      
      _logger.info('?? Connecting to: $url');

      // Build connection
      _hubConnection = HubConnectionBuilder()
          .withUrl(
            url,
            HttpConnectionOptions(
              accessTokenFactory: () async => _token,
              logMessageContent: true,
              transport: HttpTransportType.WebSockets,
            ),
          )
          .withAutomaticReconnect(
            retryDelays: [0, 1000, 3000, 5000, 10000],
          )
          .build();

      // Setup event listeners
      _setupEventListeners();

      // Start connection
      await _hubConnection.start();
      
      _logger.info('? Connected to SignalR Hub');
      connectionStatusStream.add('Connected');
    } catch (e) {
      _logger.severe('? Connection failed: $e');
      connectionStatusStream.add('Disconnected');
      rethrow;
    }
  }

  /// Setup listeners for server-sent events
  void _setupEventListeners() {
    // ?? Listen for new comments (CREATE)
    _hubConnection.on('ReceiveComment', (message) {
      try {
        final commentJson = message?[0] as Map<String, dynamic>;
        final comment = CommentModel.fromJson(commentJson);
        _logger.info('?? Comment received: ${comment.id}');
        commentCreatedStream.add(comment);
      } catch (e) {
        _logger.severe('? Error parsing comment: $e');
        errorsStream.add(['Failed to parse comment']);
      }
    });

    // ?? Listen for edited comments (UPDATE)
    _hubConnection.on('ReceiveEditedComment', (message) {
      try {
        final commentJson = message?[0] as Map<String, dynamic>;
        final comment = CommentModel.fromJson(commentJson);
        _logger.info('?? Comment edited: ${comment.id}');
        commentEditedStream.add(comment);
      } catch (e) {
        _logger.severe('? Error parsing edited comment: $e');
        errorsStream.add(['Failed to parse edited comment']);
      }
    });

    // ??? Listen for deleted comments (DELETE)
    _hubConnection.on('ReceiveDeletedComment', (message) {
      try {
        final commentId = message?[0] as String;
        _logger.info('??? Comment deleted: $commentId');
        commentDeletedStream.add(commentId);
      } catch (e) {
        _logger.severe('? Error parsing deleted comment: $e');
        errorsStream.add(['Failed to parse deleted comment']);
      }
    });

    // ?? Listen for validation/server errors
    _hubConnection.on('ReceiveErrors', (message) {
      try {
        final errors = (message?[0] as List).cast<String>();
        _logger.warning('?? Server errors: $errors');
        errorsStream.add(errors);
      } catch (e) {
        _logger.severe('? Error parsing errors: $e');
      }
    });

    // Connection state events
    _hubConnection.onclose(({error}) {
      _logger.warning('? Connection closed: $error');
      connectionStatusStream.add('Disconnected');
    });

    _hubConnection.onreconnecting(({error}) {
      _logger.info('?? Reconnecting...');
      connectionStatusStream.add('Reconnecting');
    });

    _hubConnection.onreconnected(({connectionId}) {
      _logger.info('? Reconnected with ID: $connectionId');
      connectionStatusStream.add('Connected');
    });
  }

  /// Create a new comment
  /// 
  /// Parameters:
  ///   - userComment: The comment text (max 500 characters)
  /// 
  /// Throws: Exception if not connected or invocation fails
  Future<void> createComment(String userComment) async {
    try {
      if (!isConnected) {
        throw Exception('Not connected to SignalR');
      }

      if (userComment.isEmpty) {
        throw Exception('Comment cannot be empty');
      }

      if (userComment.length > 500) {
        throw Exception('Comment must be 500 characters or less');
      }

      _logger.info('?? Creating comment...');
      await _hubConnection.invoke('CreateComment', args: [
        {
          'userComment': userComment,
        }
      ]);
      _logger.info('? Create comment sent');
    } catch (e) {
      _logger.severe('? Error creating comment: $e');
      errorsStream.add(['Failed to create comment: $e']);
      rethrow;
    }
  }

  /// Edit an existing comment
  /// 
  /// Parameters:
  ///   - commentId: ID of comment to edit
  ///   - newText: New comment text (max 500 characters)
  /// 
  /// Throws: Exception if not connected, not editable, or invocation fails
  Future<void> editComment(String commentId, String newText) async {
    try {
      if (!isConnected) {
        throw Exception('Not connected to SignalR');
      }

      if (newText.isEmpty) {
        throw Exception('Comment cannot be empty');
      }

      if (newText.length > 500) {
        throw Exception('Comment must be 500 characters or less');
      }

      _logger.info('?? Editing comment: $commentId');
      await _hubConnection.invoke('EditComment', args: [
        {
          'commentId': commentId,
          'userComment': newText,
        }
      ]);
      _logger.info('? Edit comment sent');
    } catch (e) {
      _logger.severe('? Error editing comment: $e');
      errorsStream.add(['Failed to edit comment: $e']);
      rethrow;
    }
  }

  /// Delete a comment
  /// 
  /// Parameters:
  ///   - commentId: ID of comment to delete
  /// 
  /// Throws: Exception if not connected or invocation fails
  Future<void> deleteComment(String commentId) async {
    try {
      if (!isConnected) {
        throw Exception('Not connected to SignalR');
      }

      _logger.info('??? Deleting comment: $commentId');
      await _hubConnection.invoke('DeleteComment', args: [commentId]);
      _logger.info('? Delete comment sent');
    } catch (e) {
      _logger.severe('? Error deleting comment: $e');
      errorsStream.add(['Failed to delete comment: $e']);
      rethrow;
    }
  }

  /// Disconnect from SignalR Hub
  Future<void> disconnect() async {
    try {
      await _hubConnection.stop();
      _logger.info('? Disconnected from SignalR');
      connectionStatusStream.add('Disconnected');
    } catch (e) {
      _logger.severe('? Error disconnecting: $e');
    }
  }

  /// Clean up resources
  void dispose() {
    commentCreatedStream.close();
    commentEditedStream.close();
    commentDeletedStream.close();
    connectionStatusStream.close();
    errorsStream.close();
  }
}
```

---

### Part 3: BLoC (State Management)

#### lib/bloc/comment_event.dart

```dart
import 'package:equatable/equatable.dart';
import '../models/comment_model.dart';

/// Base class for all comment events
abstract class CommentEvent extends Equatable {
  const CommentEvent();

  @override
  List<Object?> get props => [];
}

/// Initiate connection to SignalR
class ConnectEvent extends CommentEvent {
  const ConnectEvent();
}

/// Disconnect from SignalR
class DisconnectEvent extends CommentEvent {
  const DisconnectEvent();
}

/// Create a new comment
class CreateCommentEvent extends CommentEvent {
  final String userComment;

  const CreateCommentEvent(this.userComment);

  @override
  List<Object?> get props => [userComment];
}

/// Edit an existing comment
class EditCommentEvent extends CommentEvent {
  final String commentId;
  final String newText;

  const EditCommentEvent(this.commentId, this.newText);

  @override
  List<Object?> get props => [commentId, newText];
}

/// Delete a comment
class DeleteCommentEvent extends CommentEvent {
  final String commentId;

  const DeleteCommentEvent(this.commentId);

  @override
  List<Object?> get props => [commentId];
}

/// Receive a new comment from server
class CommentReceivedEvent extends CommentEvent {
  final CommentModel comment;

  const CommentReceivedEvent(this.comment);

  @override
  List<Object?> get props => [comment];
}

/// Receive an edited comment from server
class CommentEditedEvent extends CommentEvent {
  final CommentModel comment;

  const CommentEditedEvent(this.comment);

  @override
  List<Object?> get props => [comment];
}

/// Receive a deleted comment notification from server
class CommentDeletedEvent extends CommentEvent {
  final String commentId;

  const CommentDeletedEvent(this.commentId);

  @override
  List<Object?> get props => [commentId];
}
```

#### lib/bloc/comment_state.dart

```dart
import 'package:equatable/equatable.dart';
import '../models/comment_model.dart';

/// Base class for all comment states
abstract class CommentState extends Equatable {
  const CommentState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class CommentInitial extends CommentState {
  const CommentInitial();
}

/// Connecting to SignalR
class CommentConnecting extends CommentState {
  const CommentConnecting();
}

/// Connected and ready to use
class CommentConnected extends CommentState {
  final List<CommentModel> comments;

  const CommentConnected(this.comments);

  /// Create a copy with updated comments
  CommentConnected copyWith({List<CommentModel>? comments}) {
    return CommentConnected(comments ?? this.comments);
  }

  @override
  List<Object?> get props => [comments];
}

/// Disconnected from SignalR
class CommentDisconnected extends CommentState {
  const CommentDisconnected();
}

/// Loading (during create/edit/delete)
class CommentLoading extends CommentState {
  final String operation; // 'create', 'edit', 'delete'

  const CommentLoading(this.operation);

  @override
  List<Object?> get props => [operation];
}

/// Error state
class CommentError extends CommentState {
  final String message;
  final String? operation;

  const CommentError(this.message, {this.operation});

  @override
  List<Object?> get props => [message, operation];
}

/// Success state
class CommentSuccess extends CommentState {
  final String message;
  final String operation; // 'created', 'edited', 'deleted'

  const CommentSuccess(this.message, this.operation);

  @override
  List<Object?> get props => [message, operation];
}
```

#### lib/bloc/comment_bloc.dart

```dart
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:logging/logging.dart';
import '../services/signalr_service.dart';
import '../models/comment_model.dart';
import 'comment_event.dart';
import 'comment_state.dart';

/// BLoC for managing comments state and logic
class CommentBloc extends Bloc<CommentEvent, CommentState> {
  final SignalRService signalRService;
  final _logger = Logger('CommentBloc');
  
  // Maintain list of comments in memory
  final List<CommentModel> _comments = [];

  CommentBloc({required this.signalRService}) : super(const CommentInitial()) {
    // Register event handlers
    on<ConnectEvent>(_onConnect);
    on<DisconnectEvent>(_onDisconnect);
    on<CreateCommentEvent>(_onCreateComment);
    on<EditCommentEvent>(_onEditComment);
    on<DeleteCommentEvent>(_onDeleteComment);
    on<CommentReceivedEvent>(_onCommentReceived);
    on<CommentEditedEvent>(_onCommentEdited);
    on<CommentDeletedEvent>(_onCommentDeleted);

    // Setup service listeners
    _setupServiceListeners();
  }

  /// Setup listeners for SignalR service events
  void _setupServiceListeners() {
    // Listen for new comments from server
    signalRService.commentCreatedStream.stream.listen((comment) {
      add(CommentReceivedEvent(comment));
    });

    // Listen for edited comments from server
    signalRService.commentEditedStream.stream.listen((comment) {
      add(CommentEditedEvent(comment));
    });

    // Listen for deleted comments from server
    signalRService.commentDeletedStream.stream.listen((commentId) {
      add(CommentDeletedEvent(commentId));
    });

    // Listen for errors from server
    signalRService.errorsStream.stream.listen((errors) {
      final errorMessage = errors.join(', ');
      emit(CommentError(errorMessage));
    });
  }

  /// Handle ConnectEvent
  Future<void> _onConnect(ConnectEvent event, Emitter<CommentState> emit) async {
    try {
      emit(const CommentConnecting());
      await signalRService.connect();
      emit(CommentConnected(_comments));
      _logger.info('? Connected');
    } catch (e) {
      _logger.severe('? Connection failed: $e');
      emit(CommentError('Failed to connect: $e'));
    }
  }

  /// Handle DisconnectEvent
  Future<void> _onDisconnect(DisconnectEvent event, Emitter<CommentState> emit) async {
    try {
      await signalRService.disconnect();
      emit(const CommentDisconnected());
      _logger.info('? Disconnected');
    } catch (e) {
      _logger.severe('? Disconnect failed: $e');
      emit(CommentError('Failed to disconnect: $e'));
    }
  }

  /// Handle CreateCommentEvent
  Future<void> _onCreateComment(CreateCommentEvent event, Emitter<CommentState> emit) async {
    try {
      emit(const CommentLoading('create'));
      await signalRService.createComment(event.userComment);
      // Comment will be added via ReceiveComment event
    } catch (e) {
      _logger.severe('? Create failed: $e');
      emit(CommentError('Failed to create comment: $e', operation: 'create'));
    }
  }

  /// Handle EditCommentEvent
  Future<void> _onEditComment(EditCommentEvent event, Emitter<CommentState> emit) async {
    try {
      emit(const CommentLoading('edit'));
      await signalRService.editComment(event.commentId, event.newText);
      // Comment will be updated via ReceiveEditedComment event
    } catch (e) {
      _logger.severe('? Edit failed: $e');
      emit(CommentError('Failed to edit comment: $e', operation: 'edit'));
    }
  }

  /// Handle DeleteCommentEvent
  Future<void> _onDeleteComment(DeleteCommentEvent event, Emitter<CommentState> emit) async {
    try {
      emit(const CommentLoading('delete'));
      await signalRService.deleteComment(event.commentId);
      // Comment will be removed via ReceiveDeletedComment event
    } catch (e) {
      _logger.severe('? Delete failed: $e');
      emit(CommentError('Failed to delete comment: $e', operation: 'delete'));
    }
  }

  /// Handle CommentReceivedEvent (CREATE)
  Future<void> _onCommentReceived(CommentReceivedEvent event, Emitter<CommentState> emit) async {
    // Add to beginning of list (newest first)
    _comments.insert(0, event.comment);
    _logger.info('? Comment added locally: ${event.comment.id}');
    
    if (state is CommentConnected) {
      emit(CommentConnected(List.from(_comments)));
    }
  }

  /// Handle CommentEditedEvent (UPDATE)
  Future<void> _onCommentEdited(CommentEditedEvent event, Emitter<CommentState> emit) async {
    final index = _comments.indexWhere((c) => c.id == event.comment.id);
    if (index != -1) {
      _comments[index] = event.comment;
      _logger.info('? Comment updated locally: ${event.comment.id}');
      emit(CommentConnected(List.from(_comments)));
    }
  }

  /// Handle CommentDeletedEvent (DELETE)
  Future<void> _onCommentDeleted(CommentDeletedEvent event, Emitter<CommentState> emit) async {
    _comments.removeWhere((c) => c.id == event.commentId);
    _logger.info('? Comment deleted locally: ${event.commentId}');
    
    if (state is CommentConnected) {
      emit(CommentConnected(List.from(_comments)));
    }
  }

  /// Clean up resources
  @override
  Future<void> close() {
    signalRService.dispose();
    return super.close();
  }
}
```

---

### Part 4: UI Widgets

#### lib/pages/comment_page.dart

```dart
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../bloc/comment_bloc.dart';
import '../bloc/comment_event.dart';
import '../bloc/comment_state.dart';
import '../models/comment_model.dart';
import '../widgets/comment_card.dart';
import '../widgets/date_separator.dart';
import '../widgets/comment_input.dart';
import '../widgets/connection_status_bar.dart';

class CommentPage extends StatefulWidget {
  final String planId;
  final String userId;

  const CommentPage({
    required this.planId,
    required this.userId,
    Key? key,
  }) : super(key: key);

  @override
  State<CommentPage> createState() => _CommentPageState();
}

class _CommentPageState extends State<CommentPage> {
  late final CommentBloc _commentBloc;
  final TextEditingController _commentController = TextEditingController();
  String? _lastDisplayedDate;

  @override
  void initState() {
    super.initState();
    _commentBloc = context.read<CommentBloc>();
    _commentBloc.add(const ConnectEvent());
  }

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  void _handleSendComment() {
    final text = _commentController.text.trim();
    
    if (text.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter a comment')),
      );
      return;
    }

    if (text.length > 500) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Comment must be 500 characters or less')),
      );
      return;
    }

    _commentBloc.add(CreateCommentEvent(text));
    _commentController.clear();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('?? Comments'),
        centerTitle: true,
        backgroundColor: Colors.blue,
        elevation: 0,
      ),
      body: BlocListener<CommentBloc, CommentState>(
        listener: (context, state) {
          if (state is CommentError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
                duration: const Duration(seconds: 3),
              ),
            );
          }
        },
        child: Column(
          children: [
            // Connection Status Bar
            BlocBuilder<CommentBloc, CommentState>(
              builder: (context, state) {
                if (state is CommentConnecting) {
                  return ConnectionStatusBar(
                    status: 'Connecting...',
                    backgroundColor: Colors.orange,
                  );
                } else if (state is CommentConnected) {
                  return ConnectionStatusBar(
                    status: 'Connected',
                    backgroundColor: Colors.green,
                  );
                } else if (state is CommentDisconnected) {
                  return ConnectionStatusBar(
                    status: 'Disconnected',
                    backgroundColor: Colors.red,
                  );
                }
                return const SizedBox();
              },
            ),

            // Comments List
            Expanded(
              child: BlocBuilder<CommentBloc, CommentState>(
                builder: (context, state) {
                  if (state is CommentInitial || state is CommentConnecting) {
                    return const Center(
                      child: CircularProgressIndicator(),
                    );
                  } else if (state is CommentConnected) {
                    if (state.comments.isEmpty) {
                      return Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.comment,
                              size: 64,
                              color: Colors.grey[300],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              'No comments yet',
                              style: Theme.of(context).textTheme.titleLarge,
                            ),
                            const SizedBox(height: 8),
                            Text(
                              'Start the conversation!',
                              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Colors.grey,
                              ),
                            ),
                          ],
                        ),
                      );
                    }

                    return ListView.builder(
                      reverse: true,
                      itemCount: state.comments.length,
                      padding: const EdgeInsets.symmetric(vertical: 8),
                      itemBuilder: (context, index) {
                        final comment = state.comments[index];
                        final previousComment = index < state.comments.length - 1
                            ? state.comments[index + 1]
                            : null;

                        // Check if date separator needed
                        final showDateSeparator = _shouldShowDateSeparator(
                          previousComment,
                          comment,
                        );

                        return Column(
                          children: [
                            if (showDateSeparator)
                              DateSeparator(date: comment.getDateDisplay()),
                            CommentCard(
                              comment: comment,
                              isOwnComment: comment.userId == widget.userId,
                              onEdit: (newText) {
                                _commentBloc.add(
                                  EditCommentEvent(comment.id, newText),
                                );
                              },
                              onDelete: () {
                                _commentBloc.add(
                                  DeleteCommentEvent(comment.id),
                                );
                              },
                            ),
                          ],
                        );
                      },
                    );
                  } else if (state is CommentError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.error,
                            size: 64,
                            color: Colors.red[300],
                          ),
                          const SizedBox(height: 16),
                          Text('Error: ${state.message}'),
                          const SizedBox(height: 16),
                          ElevatedButton(
                            onPressed: () {
                              _commentBloc.add(const ConnectEvent());
                            },
                            child: const Text('Retry'),
                          ),
                        ],
                      ),
                    );
                  }
                  return const SizedBox();
                },
              ),
            ),

            // Input Area
            SafeArea(
              child: CommentInput(
                controller: _commentController,
                onSend: _handleSendComment,
                enabled: context.read<CommentBloc>().state is CommentConnected,
              ),
            ),
          ],
        ),
      ),
    );
  }

  bool _shouldShowDateSeparator(CommentModel? previous, CommentModel current) {
    if (previous == null) return true;
    
    final prevDate = '${previous.createdAt.year}-${previous.createdAt.month}-${previous.createdAt.day}';
    final currDate = '${current.createdAt.year}-${current.createdAt.month}-${current.createdAt.day}';
    
    return prevDate != currDate;
  }
}
```

#### lib/widgets/comment_card.dart

```dart
import 'package:flutter/material.dart';
import '../models/comment_model.dart';
import 'edit_comment_dialog.dart';

class CommentCard extends StatelessWidget {
  final CommentModel comment;
  final bool isOwnComment;
  final Function(String) onEdit;
  final VoidCallback onDelete;

  const CommentCard({
    required this.comment,
    required this.isOwnComment,
    required this.onEdit,
    required this.onDelete,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      decoration: BoxDecoration(
        color: comment.isUpdated ? Colors.blue[50] : Colors.grey[100],
        borderRadius: BorderRadius.circular(8),
        border: Border(
          left: BorderSide(
            color: Colors.blue,
            width: 4,
          ),
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Left side: User badge + Time
            Column(
              mainAxisAlignment: MainAxisAlignment.start,
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 3),
                  decoration: BoxDecoration(
                    color: Colors.yellow[200],
                    borderRadius: BorderRadius.circular(3),
                  ),
                  child: Text(
                    comment.userIdShort,
                    style: const TextStyle(
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF92400E),
                    ),
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  comment.getTimeDisplay(),
                  style: const TextStyle(
                    fontSize: 11,
                    color: Color(0xFF666666),
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
            const SizedBox(width: 12),

            // Right side: Content
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Header with badges
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.blue,
                          borderRadius: BorderRadius.circular(3),
                        ),
                        child: const Text(
                          'User',
                          style: TextStyle(
                            fontSize: 11,
                            fontWeight: FontWeight.w600,
                            color: Colors.white,
                          ),
                        ),
                      ),
                      if (comment.isUpdated) ...[
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 6,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.blue[100],
                            borderRadius: BorderRadius.circular(3),
                          ),
                          child: const Text(
                            '[EDITED]',
                            style: TextStyle(
                              fontSize: 9,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF2C5282),
                            ),
                          ),
                        ),
                      ],
                    ],
                  ),
                  const SizedBox(height: 8),

                  // Message content
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(3),
                    ),
                    child: Text(
                      comment.userComment,
                      style: const TextStyle(
                        fontSize: 13,
                        height: 1.4,
                        color: Color(0xFF333333),
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),

                  // Action buttons (for own comments only)
                  if (isOwnComment)
                    Row(
                      children: [
                        if (comment.canEdit())
                          TextButton.icon(
                            onPressed: () => _showEditDialog(context),
                            icon: const Icon(Icons.edit, size: 16),
                            label: const Text('Edit'),
                            style: TextButton.styleFrom(
                              foregroundColor: Colors.blue,
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                              ),
                              minimumSize: Size.zero,
                              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                            ),
                          ),
                        if (comment.canEdit()) const SizedBox(width: 8),
                        TextButton.icon(
                          onPressed: () => _showDeleteConfirmation(context),
                          icon: const Icon(Icons.delete, size: 16),
                          label: const Text('Delete'),
                          style: TextButton.styleFrom(
                            foregroundColor: Colors.red,
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                            ),
                            minimumSize: Size.zero,
                            tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                          ),
                        ),
                      ],
                    ),

                  // Edit timeout warning
                  if (isOwnComment && !comment.canEdit())
                    Container(
                      margin: const EdgeInsets.only(top: 8),
                      padding: const EdgeInsets.all(6),
                      decoration: BoxDecoration(
                        color: Colors.red[50],
                        borderRadius: BorderRadius.circular(3),
                      ),
                      child: const Text(
                        '?? Edit time expired (5 min limit)',
                        style: TextStyle(
                          fontSize: 10,
                          color: Color(0xFFC53030),
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showEditDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => EditCommentDialog(
        initialText: comment.userComment,
        onSave: (newText) {
          onEdit(newText);
          Navigator.pop(context);
        },
      ),
    );
  }

  void _showDeleteConfirmation(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete Comment'),
        content: const Text('Are you sure you want to delete this comment?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () {
              onDelete();
              Navigator.pop(context);
            },
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
  }
}
```

#### lib/widgets/date_separator.dart

```dart
import 'package:flutter/material.dart';

class DateSeparator extends StatelessWidget {
  final String date;

  const DateSeparator({
    required this.date,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 16),
      child: Row(
        children: [
          const Expanded(
            child: Divider(
              color: Colors.grey,
              height: 1,
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              date.toUpperCase(),
              style: const TextStyle(
                color: Colors.grey,
                fontSize: 11,
                fontWeight: FontWeight.w600,
                letterSpacing: 0.5,
              ),
            ),
          ),
          const Expanded(
            child: Divider(
              color: Colors.grey,
              height: 1,
            ),
          ),
        ],
      ),
    );
  }
}
```

#### lib/widgets/comment_input.dart

```dart
import 'package:flutter/material.dart';

class CommentInput extends StatefulWidget {
  final TextEditingController controller;
  final VoidCallback onSend;
  final bool enabled;

  const CommentInput({
    required this.controller,
    required this.onSend,
    required this.enabled,
    Key? key,
  }) : super(key: key);

  @override
  State<CommentInput> createState() => _CommentInputState();
}

class _CommentInputState extends State<CommentInput> {
  int _charCount = 0;

  @override
  void initState() {
    super.initState();
    widget.controller.addListener(_updateCharCount);
  }

  void _updateCharCount() {
    setState(() {
      _charCount = widget.controller.text.length;
    });
  }

  @override
  void dispose() {
    widget.controller.removeListener(_updateCharCount);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        border: Border(
          top: BorderSide(
            color: Colors.grey[300]!,
          ),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          TextField(
            controller: widget.controller,
            enabled: widget.enabled,
            maxLines: null,
            maxLength: 500,
            minLines: 1,
            decoration: InputDecoration(
              hintText: 'Type a comment...',
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(8),
              ),
              filled: true,
              fillColor: Colors.grey[100],
              contentPadding: const EdgeInsets.all(12),
              suffixIcon: widget.enabled
                  ? IconButton(
                      icon: const Icon(Icons.send),
                      onPressed: widget.onSend,
                      color: Colors.blue,
                    )
                  : null,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            '$_charCount/500',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[600],
            ),
          ),
        ],
      ),
    );
  }
}
```

#### lib/widgets/connection_status_bar.dart

```dart
import 'package:flutter/material.dart';

class ConnectionStatusBar extends StatelessWidget {
  final String status;
  final Color backgroundColor;

  const ConnectionStatusBar({
    required this.status,
    required this.backgroundColor,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      color: backgroundColor,
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          if (status == 'Connecting...' || status == 'Connecting')
            const SizedBox(
              width: 16,
              height: 16,
              child: CircularProgressIndicator(
                strokeWidth: 2,
                valueColor: AlwaysStoppedAnimation(Colors.white),
              ),
            )
          else
            Container(
              width: 12,
              height: 12,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white,
              ),
            ),
          const SizedBox(width: 8),
          Text(
            status,
            style: const TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
```

#### lib/widgets/edit_comment_dialog.dart

```dart
import 'package:flutter/material.dart';

class EditCommentDialog extends StatefulWidget {
  final String initialText;
  final Function(String) onSave;

  const EditCommentDialog({
    required this.initialText,
    required this.onSave,
    Key? key,
  }) : super(key: key);

  @override
  State<EditCommentDialog> createState() => _EditCommentDialogState();
}

class _EditCommentDialogState extends State<EditCommentDialog> {
  late TextEditingController _controller;
  int _charCount = 0;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialText);
    _charCount = widget.initialText.length;
    _controller.addListener(_updateCharCount);
  }

  void _updateCharCount() {
    setState(() {
      _charCount = _controller.text.length;
    });
  }

  @override
  void dispose() {
    _controller.removeListener(_updateCharCount);
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('?? Edit Comment'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextField(
            controller: _controller,
            maxLines: null,
            maxLength: 500,
            decoration: InputDecoration(
              hintText: 'Edit your comment',
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(8),
              ),
              helperText: '$_charCount/500',
            ),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: () {
            final newText = _controller.text.trim();
            if (newText.isEmpty) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Comment cannot be empty')),
              );
              return;
            }
            widget.onSave(newText);
          },
          child: const Text('Save'),
        ),
      ],
    );
  }
}
```

---

### Part 5: Main App

#### lib/main.dart

```dart
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:logging/logging.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'services/signalr_service.dart';
import 'bloc/comment_bloc.dart';
import 'pages/comment_page.dart';

void main() {
  _setupLogging();
  runApp(const MyApp());
}

/// Setup logging for debugging
void _setupLogging() {
  Logger.root.level = Level.ALL;
  Logger.root.onRecord.listen((record) {
    debugPrint(
      '${record.level.name}: ${record.time}: ${record.message}',
    );
    if (record.error != null) {
      debugPrint('? Error: ${record.error}');
    }
  });
}

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Comment Hub',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        useMaterial3: true,
      ),
      home: const LoginPage(),
      debugShowCheckedModeBanner: false,
    );
  }
}

/// Login page to configure connection
class LoginPage extends StatefulWidget {
  const LoginPage({Key? key}) : super(key: key);

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _jwtController = TextEditingController();
  final _planIdController = TextEditingController(text: 'plan-001');
  final _serverUrlController =
      TextEditingController(text: 'https://localhost:7264');
  bool _isLoading = false;

  @override
  void dispose() {
    _jwtController.dispose();
    _planIdController.dispose();
    _serverUrlController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('?? SignalR Comment Hub'),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.chat_bubble,
              size: 80,
              color: Colors.blue[300],
            ),
            const SizedBox(height: 32),
            
            TextField(
              controller: _serverUrlController,
              decoration: InputDecoration(
                labelText: 'Server URL',
                hintText: 'https://localhost:7264',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                prefixIcon: const Icon(Icons.language),
              ),
            ),
            const SizedBox(height: 16),
            
            TextField(
              controller: _planIdController,
              decoration: InputDecoration(
                labelText: 'Plan ID',
                hintText: 'plan-001',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                prefixIcon: const Icon(Icons.library_books),
              ),
            ),
            const SizedBox(height: 16),
            
            TextField(
              controller: _jwtController,
              decoration: InputDecoration(
                labelText: 'JWT Token',
                hintText: 'Paste your JWT token here',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                prefixIcon: const Icon(Icons.lock),
                helperText: 'Get from your authentication endpoint',
              ),
              maxLines: 4,
            ),
            const SizedBox(height: 32),
            
            SizedBox(
              width: double.infinity,
              height: 56,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _handleConnect,
                child: _isLoading
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                        ),
                      )
                    : const Text(
                        'Connect to Hub',
                        style: TextStyle(fontSize: 16),
                      ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _handleConnect() async {
    final serverUrl = _serverUrlController.text.trim();
    final planId = _planIdController.text.trim();
    final jwtToken = _jwtController.text.trim();

    // Validation
    if (serverUrl.isEmpty) {
      _showError('Please enter server URL');
      return;
    }
    if (planId.isEmpty) {
      _showError('Please enter plan ID');
      return;
    }
    if (jwtToken.isEmpty) {
      _showError('Please enter JWT token');
      return;
    }

    setState(() => _isLoading = true);

    try {
      // Extract user ID from JWT
      final userId = _extractUserIdFromToken(jwtToken);
      if (userId.isEmpty) {
        throw Exception('Could not extract user ID from token');
      }

      // Create SignalR service
      final signalRService = SignalRService(
        baseUrl: serverUrl,
        token: jwtToken,
        planId: planId,
      );

      // Navigate to comment page
      if (mounted) {
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (context) => BlocProvider(
              create: (context) => CommentBloc(signalRService: signalRService),
              child: CommentPage(
                planId: planId,
                userId: userId,
              ),
            ),
          ),
        );
      }
    } catch (e) {
      _showError('Error: $e');
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  String _extractUserIdFromToken(String token) {
    try {
      final decodedToken = JwtDecoder.decode(token);
      return decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
          'unknown';
    } catch (e) {
      return '';
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: Colors.red,
        duration: const Duration(seconds: 3),
      ),
    );
  }
}
```

---

## ?? API Reference

### SignalRService Methods

#### `connect()`
Connects to the SignalR hub.

**Example:**
```dart
await signalRService.connect();
```

#### `createComment(String userComment)`
Creates a new comment.

**Parameters:**
- `userComment` (String, required): Comment text (max 500 characters)

**Example:**
```dart
await signalRService.createComment('Hello, this is my comment!');
```

#### `editComment(String commentId, String newText)`
Edits an existing comment (must be within 5 minutes).

**Parameters:**
- `commentId` (String, required): ID of comment to edit
- `newText` (String, required): New comment text (max 500 characters)

**Example:**
```dart
await signalRService.editComment('123', 'Updated comment text');
```

#### `deleteComment(String commentId)`
Deletes a comment.

**Parameters:**
- `commentId` (String, required): ID of comment to delete

**Example:**
```dart
await signalRService.deleteComment('123');
```

#### `disconnect()`
Disconnects from the SignalR hub.

**Example:**
```dart
await signalRService.disconnect();
```

---

### CommentBloc Events

| Event | Purpose | Parameters |
|-------|---------|-----------|
| `ConnectEvent()` | Connect to hub | - |
| `DisconnectEvent()` | Disconnect from hub | - |
| `CreateCommentEvent(String)` | Create comment | `userComment` |
| `EditCommentEvent(String, String)` | Edit comment | `commentId`, `newText` |
| `DeleteCommentEvent(String)` | Delete comment | `commentId` |

**Example:**
```dart
// Connect
_commentBloc.add(const ConnectEvent());

// Create comment
_commentBloc.add(const CreateCommentEvent('Hello'));

// Edit comment
_commentBloc.add(EditCommentEvent('123', 'Updated'));

// Delete comment
_commentBloc.add(const DeleteCommentEvent('123'));
```

---

## ?? Testing

### Unit Test Example

```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';

void main() {
  group('CommentBloc', () {
    late MockSignalRService mockSignalRService;
    late CommentBloc commentBloc;

    setUp(() {
      mockSignalRService = MockSignalRService();
      commentBloc = CommentBloc(signalRService: mockSignalRService);
    });

    tearDown(() {
      commentBloc.close();
    });

    // Test connection
    blocTest<CommentBloc, CommentState>(
      'emits [CommentConnecting, CommentConnected] when connecting',
      build: () => commentBloc,
      act: (bloc) => bloc.add(const ConnectEvent()),
      expect: () => [
        const CommentConnecting(),
        const CommentConnected([]),
      ],
    );

    // Test create comment
    blocTest<CommentBloc, CommentState>(
      'creates comment successfully',
      build: () => commentBloc,
      act: (bloc) {
        bloc.add(const CreateCommentEvent('Test'));
      },
      verify: (_) {
        verify(mockSignalRService.createComment('Test')).called(1);
      },
    );
  });
}

class MockSignalRService extends Mock implements SignalRService {}
```

---

## ?? Troubleshooting

### Issue 1: Connection Fails

**Problem:** `? Connection failed`

**Solution:**
```dart
// Check 1: Verify server URL
final url = 'https://your-server:7264/hubs/comments';

// Check 2: Verify JWT token is valid
final isValid = !JwtDecoder.isExpired(token);

// Check 3: Verify plan ID exists
// Check 4: Enable logging to see details
Logger.root.level = Level.ALL;
```

### Issue 2: Messages Not Received

**Problem:** Comments appear on web but not in Flutter

**Solution:**
```dart
// Check deserialization
try {
  final json = jsonDecode(response);
  final comment = CommentModel.fromJson(json);
  print('? Deserialized: ${comment.id}');
} catch (e) {
  print('? Deserialization error: $e');
  // Verify field names match:
  // - id, userComment, userId, planId, createdAt, updatedAt
}
```

### Issue 3: 5-Minute Edit Not Working

**Problem:** Can still edit after 5 minutes

**Solution:**
```dart
// Check timestamp is correct
bool canEdit = comment.canEdit(); // should be false after 5 min

// Verify: Comment.createdAt should match server time
// Make sure device time is synced
```

### Issue 4: JWT Token Expired

**Problem:** `Token expired` error

**Solution:**
```dart
// Refresh token before connecting
final newToken = await getNewToken();

// Then connect
await signalRService.disconnect();
signalRService = SignalRService(
  baseUrl: baseUrl,
  token: newToken,  // Use new token
  planId: planId,
);
await signalRService.connect();
```

---

## ?? Data Flow Diagram

```
User                Flutter App              Backend SignalR Hub            Database
?                        ?                           ?                        ?
?? Login ???????????????>?                           ?                        ?
?                        ?<???????? JWT Token ????????                        ?
?                        ?                           ?                        ?
?? Send Comment ????????>?                           ?                        ?
?                        ?? CreateComment ?????????>?                        ?
?                        ?                           ?? Save Comment ????????>?
?                        ?<? ReceiveComment ??????????                        ?
?                        ?<? (Broadcast to all) ?????                        ?
?                        ?                           ?<?????? Return ?????????
?                        ?                           ?                        ?
?? Edit Comment ????????>?                           ?                        ?
?                        ?? EditComment ????????????>?                        ?
?                        ?                           ?? Update Comment ??????>?
?                        ?<? ReceiveEditedComment ????                        ?
?                        ?<? (Broadcast to all) ?????                        ?
?                        ?                           ?<?????? Return ?????????
?                        ?                           ?                        ?
?? Delete Comment ??????>?                           ?                        ?
?                        ?? DeleteComment ???????????>?                        ?
?                        ?                           ?? Delete Comment ??????>?
?                        ?<? ReceiveDeletedComment ???                        ?
?                        ?<? (Broadcast to all) ?????                        ?
?                        ?                           ?<?????? Return ?????????
```

---

## ?? Key Differences: Flutter vs Web

| Aspect | Web (JavaScript) | Flutter (Dart) |
|--------|------------------|-----------------|
| **Package** | `@microsoft/signalr` | `signalr_netcore` |
| **State Management** | Plain objects | BLoC pattern |
| **UI Framework** | HTML/CSS | Flutter Widgets |
| **Date Handling** | JavaScript Date | Dart DateTime |
| **JSON Serialization** | Manual | `json_annotation` |
| **Async/Await** | Same | Same |
| **Build Command** | `npm build` | `flutter build` |

---

## ?? Complete File Structure

```
lib/
??? main.dart                          # App entry point
??? models/
?   ??? comment_model.dart            # Comment data model
?   ??? dtos.dart                     # Data transfer objects
??? services/
?   ??? signalr_service.dart          # SignalR communication
??? bloc/
?   ??? comment_bloc.dart             # Business logic
?   ??? comment_event.dart            # Events
?   ??? comment_state.dart            # States
??? pages/
?   ??? comment_page.dart             # Main page
??? widgets/
    ??? comment_card.dart             # Comment display
    ??? comment_input.dart            # Input field
    ??? date_separator.dart           # Date separator
    ??? connection_status_bar.dart    # Connection indicator
    ??? edit_comment_dialog.dart      # Edit dialog
```

---

## ? Complete Checklist

- [ ] Created Flutter project
- [ ] Added all required packages to `pubspec.yaml`
- [ ] Created model files (`comment_model.dart`, `dtos.dart`)
- [ ] Created SignalR service (`signalr_service.dart`)
- [ ] Created BLoC files (event, state, bloc)
- [ ] Created UI pages and widgets
- [ ] Set up `main.dart` with login page
- [ ] Obtained JWT token from backend
- [ ] Updated server URL and plan ID
- [ ] Run `flutter pub get`
- [ ] Run `flutter pub run build_runner build`
- [ ] Run `flutter run`
- [ ] Test connection
- [ ] Test create comment
- [ ] Test edit comment (within 5 minutes)
- [ ] Test delete comment
- [ ] Test 5-minute edit timeout
- [ ] Deploy to device/emulator

---

## ?? Quick Commands

```bash
# Create project
flutter create comment_hub_app
cd comment_hub_app

# Get packages
flutter pub get

# Generate JSON serialization
flutter pub run build_runner build

# Run app
flutter run

# Run tests
flutter test

# Build APK (Android)
flutter build apk

# Build IPA (iOS)
flutter build ios
```

---

## ?? Support Resources

- [SignalR .NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [Flutter BLoC Documentation](https://bloclibrary.dev/)
- [Dart Documentation](https://dart.dev/guides)
- [Flutter Documentation](https://flutter.dev/docs)

---

## ?? Notes

- **Timestamps**: All times are in UTC from server. Convert to local as needed.
- **Edit Window**: 5-minute window for editing is enforced on server. UI shows warning after.
- **Real-time**: SignalR automatically handles reconnection with exponential backoff.
- **Memory**: Dispose streams properly to prevent memory leaks.
- **Performance**: Store comments locally if needed for offline support.

---

**Version:** 1.0  
**Status:** ? Production Ready  
**Last Updated:** 2025-03-26  
**Framework:** Flutter 3.0+, Dart 3.0+, .NET 8 SignalR

---

**Need help?** Check the troubleshooting section above or review the code comments.
