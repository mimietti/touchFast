/*
*  UnitySocialSDK
*
*  Version: 0.2.44
*  Date: 2016-12-19T13:00:55Z
*  Copyright (c) 2016 Unity. All rights reserved.
*
*/

#import <UIKit/UIKit.h>

@protocol UnitySocialDelegate<NSObject>

@optional
- (void)unitySocialGameShouldPause;
- (void)unitySocialGameShouldResume;
- (void)unitySocialRewardClaimed:(NSDictionary*)metadata;
- (void)unitySocialChallengeStarted:(NSDictionary*)challenge withMetadata:(NSDictionary*)metadata;
- (void)unitySocialSendMessage:(NSDictionary*)data toDestination:(NSString*)destination;
- (void)unitySocialInitialized:(BOOL)isSupported;
- (void)unitySocialUpdateEntryPointState:(NSDictionary*)state;
@end

@interface UnitySocial : NSObject

#pragma mark - Initialization
+ (id<UnitySocialDelegate>)delegate;
+ (void)setDelegate:(id<UnitySocialDelegate>)delegate;
+ (void)setManifestServer:(NSString*)manifestServer;
+ (NSString*)getManifestServer;
+ (void)initializeWithClientId:(NSString*)clientId;
+ (void)showOnView:(UIView*)view;
+ (void)showOnView:(UIView*)view andModule:(NSString*)moduleName;
+ (void)entryPointClicked;

#pragma mark - Status
+ (BOOL)isSupported;
+ (BOOL)isReady;
+ (NSDictionary*)getEntryPointState;

#pragma mark - Look & Feel
+ (void)showNotificationActorOnLeftTop:(NSNumber*)topOffsetInPixels;
+ (void)showNotificationActorOnLeftBottom:(NSNumber*)bottomOffsetInPixels;
+ (void)showNotificationActorOnRightTop:(NSNumber*)topOffsetInPixels;
+ (void)showNotificationActorOnRightBottom:(NSNumber*)bottomOffsetInPixels;
+ (void)hideNotifications;
+ (void)enableEntryPointUpdatesWithImageSize:(NSNumber*)sizeInPhysicalPixels;
+ (void)disableEntryPointUpdates;
+ (void)setColorTheme:(NSDictionary*)theme;

#pragma mark - Sharing
+ (void)enableShareDestination:(NSString*)destination;

#pragma mark - Session control
+ (void)startSession;
+ (void)endSession:(NSDictionary*)data;

#pragma mark - User settings
+ (void)addTags:(NSArray*)tags;
+ (void)removeTags:(NSArray*)tags;

#pragma mark - URL handling
+ (BOOL)handleOpenURL:(NSURL*)url sourceApplication:(id)sourceApplication annotation:(id)annotation;

#pragma mark - Push notifications
+ (void)didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken;
+ (void)didFailToRegisterForRemoteNotificationsWithError:(NSError*)error;
+ (void)didReceiveRemoteNotification:(NSDictionary*)userInfo;
+ (void)didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings;
+ (BOOL)didFinishLaunchingWithOptions:(NSDictionary*)launchOptions;

@end
